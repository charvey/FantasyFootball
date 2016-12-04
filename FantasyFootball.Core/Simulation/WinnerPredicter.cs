using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Data;
using FantasyFootball.Core.Objects;
using FantasyFootball.Core.Simulation.Facts;
using FantasyFootball.Data.Yahoo;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FantasyFootball.Core.Simulation
{
    public class CandidateScoreProvider : ScoreProvider
    {
        private readonly Candidate candidate;
        private readonly Random random;

        public CandidateScoreProvider(Candidate candidate)
        {
            this.candidate = candidate;
            this.random = new Random();
        }

        public double GetScore(Player player, int week)
        {
            return candidate.GetFunction(new Situation(player.Id, week)).Inverse(random.NextDouble());
        }
    }

    public class WinnerPredicter
    {
        private const string league_key = "359.l.48793";
        private int CurrentWeek = SeasonWeek.Current;
        private readonly FantasySportsService service = new FantasySportsService();
        private readonly CandidateScoreProvider scoreProvider;

        public WinnerPredicter()
        {
            var candidate = new ComplexCandidate(
                new PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter(1, 0.1),
                new ByPlayerHistoricalGroupSpecifier(),
                new RawScoreModel());
            scoreProvider = new CandidateScoreProvider(candidate);
        }

        public void PredictWinners()
        {
            const int trials = 100;
            var universe = new Universe();
            StartSeason(universe);

            var stopwatch = Stopwatch.StartNew();
            var winners = new ConcurrentDictionary<Team, int>();
            Enumerable.Range(1, trials).AsParallel().ForAll(_ =>
            {
                Console.WriteLine($"Starting Trial #{_}");
                var runUniverse = universe.Clone();
                FinishSeason(runUniverse);
                winners.AddOrUpdate(runUniverse.GetChampionshipWinner(), 1, (k, c) => c + 1);
            });

            var average = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds / trials);
            Console.WriteLine($"\n{trials} trials ran in {stopwatch.Elapsed} (average {average} each)");
            foreach (var team in winners.OrderByDescending(x => x.Value))
            {
                Console.WriteLine($"{team.Key.Owner} {team.Value} {1.0 * team.Value / trials:P}");
            }
        }

        public void PredictWinner()
        {
            var universe = new Universe();
            StartSeason(universe);
            FinishSeason(universe);
            Console.WriteLine(universe.GetChampionshipWinner().Owner);
        }

        private ConcurrentDictionary<Tuple<int, int>, Player[]> pastPlayers = new ConcurrentDictionary<Tuple<int, int>, Player[]>();
        private Player[] GetPastPlayers(Team team, int week)
        {
            var key = Tuple.Create(team.Id, week);
            return pastPlayers.GetOrAdd(key, _ => service.TeamRoster($"{league_key}.t.{team.Id}", week).players
                .Where(p => p.selected_position.position != "BN")
                .Select(Players.From).ToArray());
        }

        private ConcurrentDictionary<Tuple<int, int>, Player[]> futurePlayers = new ConcurrentDictionary<Tuple<int, int>, Player[]>();
        private Player[] GetFuturePlayers(Team team, int week)
        {
            var key = Tuple.Create(team.Id, week);
            return futurePlayers.GetOrAdd(key, _ => service.TeamRoster($"{league_key}.t.{team.Id}", week).players
                .Select(Players.From).ToArray());
        }

        private void StartSeason(Universe universe)
        {
            foreach (var team in Teams.All())
                universe.AddFact(new AddTeam { Team = team });

            for (int week = 1; week < CurrentWeek; week++)
            {
                Console.WriteLine($"Recording Week #{week}");
                RecordWeek(universe, week);
            }
        }

        private void FinishSeason(Universe universe)
        {
            for (int week = CurrentWeek; week <= SeasonWeek.RegularSeasonEnd; week++)
            {
                Console.WriteLine($"Predicting Week #{week}");
                PredictWeek(universe, week);
            }

            Console.WriteLine($"Predicting Quarterfinals");
            PredictQuarterfinals(universe);
            Console.WriteLine($"Predicting Semifinals");
            PredictSemifinals(universe);
            Console.WriteLine($"Predicting Championship");
            PredictChampionship(universe);
        }

        private void RecordWeek(Universe universe, int week)
        {
            foreach (var player in Players.All())
            {
                universe.AddFact(new SetScore
                {
                    Player = player,
                    Week = week,
                    Score = DumpData.GetActualScore(player.Id, week).Value
                });
            }

            foreach (var matchup in Matchups.GetByWeek(week))
                universe.AddFact(new AddMatchup { Matchup = matchup });

            foreach (var team in universe.GetTeams())
            {
                universe.AddFact(new SetRoster
                {
                    Team = team,
                    Week = week,
                    Players = GetPastPlayers(team, week)
                });
            }
        }

        private void PredictWeek(Universe universe, int week)
        {
            foreach (var matchup in Matchups.GetByWeek(week))
                universe.AddFact(new AddMatchup { Matchup = matchup });

            PredictTeamsForWeek(universe, universe.GetTeams(), week);
        }

        private void PredictTeamsForWeek(Universe universe, IEnumerable<Team> teams, int week)
        {
            foreach (var team in teams)
            {
                var allPlayers = GetFuturePlayers(team, week);
                var roster = new RosterPicker(new DumpCsvScoreProvider()).PickRoster(allPlayers, week).ToArray();

                foreach (var player in roster)
                {
                    universe.AddFact(new SetScore
                    {
                        Player = player,
                        Week = week,
                        Score = scoreProvider.GetScore(player, week)
                    });
                }

                universe.AddFact(new SetRoster
                {
                    Team = team,
                    Week = week,
                    Players = roster
                });
            }
        }

        private void PredictQuarterfinals(Universe universe)
        {
            PredictTeamsForWeek(universe, new[] {
                universe.GetTeamInPlaceAtEndOfSeason(3),
                universe.GetTeamInPlaceAtEndOfSeason(4),
                universe.GetTeamInPlaceAtEndOfSeason(5),
                universe.GetTeamInPlaceAtEndOfSeason(6)
            }, SeasonWeek.QuarterFinalWeek);
        }

        private void PredictSemifinals(Universe universe)
        {
            PredictTeamsForWeek(universe, new[] {
                universe.GetTeamInPlaceAtEndOfSeason(1),
                universe.GetTeamInPlaceAtEndOfSeason(2),
                universe.GetQuarterFinalAWinner(),
                universe.GetQuarterFinalBWinner()
            }, SeasonWeek.SemifinalWeek);
        }

        private void PredictChampionship(Universe universe)
        {
            PredictTeamsForWeek(universe, new[] {
                universe.GetSemifinalAWinner(),
                universe.GetSemifinalBWinner()
            }, SeasonWeek.ChampionshipWeek);
        }
    }
}
