using FantasyFootball.Core.Draft;
using FantasyFootball.Data.Yahoo;
using System;
using System.Linq;
using FantasyFootball.Core.Players;
using System.Collections.Concurrent;
using System.Diagnostics;
using FantasyFootball.Core.Analysis;

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
        private const int CurrentWeek = 11;
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
            const int trials = 10;
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
            foreach(var team in winners.OrderByDescending(x => x.Value))
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

        private void StartSeason(Universe universe)
        {
            foreach (var team in Draft.Team.All())
                universe.AddFact(new AddTeam { Team = team });

            for (int week = 1; week <= CurrentWeek; week++)
            {
                Console.WriteLine($"Recording Week #{week}");
                RecordWeek(universe, week);
            }
        }

        private void FinishSeason(Universe universe)
        {
            for (int week = CurrentWeek + 1; week <= 13; week++)
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
            foreach (var player in Player.All())
            {
                universe.AddFact(new SetScore
                {
                    Player = player,
                    Week = week,
                    Score = DumpData.GetActualScore(player.Id, week).Value
                });
            }

            foreach (var matchup in Core.Matchup.Matchups(week))
                universe.AddFact(new AddMatchup { Matchup = matchup });

            foreach (var team in universe.GetTeams())
            {
                universe.AddFact(new SetRoster
                {
                    Team = team,
                    Week = week,
                    Players = service.TeamRoster($"{league_key}.t.{team.Id}", week).players
                        .Where(p => p.selected_position.position != "BN")
                        .Select(Players.Player.From).ToArray()
                });
            }
        }

        private void PredictWeek(Universe universe, int week)
        {
            foreach(var player in Player.All())
            {
                universe.AddFact(new SetScore
                {
                    Player = player,
                    Week = week,
                    Score = scoreProvider.GetScore(player, week)
                });
            }

            foreach (var matchup in Core.Matchup.Matchups(week))
                universe.AddFact(new AddMatchup { Matchup = matchup });

            foreach (var team in universe.GetTeams())
            {
                var allPlayers = service.TeamRoster($"{league_key}.t.{team.Id}", week).players.Select(Players.Player.From);
                var roster = new RosterPicker(new DataCsvScoreProvider()).PickRoster(allPlayers, week);
                universe.AddFact(new SetRoster
                {
                    Team = team,
                    Week = week,
                    Players = roster.ToArray()
                });
            }
        }

        private void PredictQuarterfinals(Universe universe)
        {
            //Does too much work
            PredictWeek(universe, 14);
        }

        private void PredictSemifinals(Universe universe)
        {
            //Does too much work
            PredictWeek(universe, 15);
        }

        private void PredictChampionship(Universe universe)
        {
            //Does too much work
            PredictWeek(universe, 16);
        }
    }
}
