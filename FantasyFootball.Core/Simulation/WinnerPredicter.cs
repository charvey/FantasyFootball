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
            const int trials = 1000;
            var universe = new Universe();
            StartSeason(universe);

            var stopwatch = Stopwatch.StartNew();
            var winners = new ConcurrentDictionary<Team, int>();
            var playoffAppearances = new ConcurrentDictionary<Team, int>();
            var finalStandings = new ConcurrentDictionary<Tuple<Team, int>, int>();

            Action<int> printProgress = (int t) =>
              {
                  var average = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds / t);
                  Console.WriteLine($"\n{t} trials ran in {stopwatch.Elapsed} (average {average} each)");
                  Console.WriteLine("\nChampionships Wins");
                  foreach (var team in winners.OrderByDescending(x => x.Value))
                      Console.WriteLine($"{team.Key.Owner} {team.Value} {1.0 * team.Value / t:P}");
                  Console.WriteLine("\nPlayoff Appearances");
                  foreach (var team in playoffAppearances.OrderByDescending(x => x.Value))
                      Console.WriteLine($"{team.Key.Owner} {team.Value} {1.0 * team.Value / t:P}");
                  Console.WriteLine("\nFinal Rankings");
                  foreach (var team in universe.GetTeams().OrderBy(team => team.Owner))
                  {
                      Console.Write(team.Owner);
                      foreach (var standing in Enumerable.Range(1, 12))
                      {
                          Console.Write($",{1.0 * finalStandings.GetOrAdd(Tuple.Create(team, standing), 0) / t:P}");
                      }
                      Console.WriteLine();
                  }
              };

            Enumerable.Range(1, trials).AsParallel().ForAll(_ =>
            {
                Console.WriteLine($"Starting Trial #{_}");
                var runUniverse = universe.Clone();
                FinishSeason(runUniverse);
                winners.AddOrUpdate(runUniverse.GetChampionshipResult().Winner, 1, (k, c) => c + 1);
                foreach (var team in runUniverse.GetStandingsAtEndOfSeason().Take(6))
                    playoffAppearances.AddOrUpdate(team, 1, (k, c) => c + 1);
                {
                    finalStandings.AddOrUpdate(Tuple.Create(runUniverse.GetChampionshipResult().Winner, 1), 1, (k, c) => c + 1);
                    finalStandings.AddOrUpdate(Tuple.Create(runUniverse.GetChampionshipResult().Loser, 2), 1, (k, c) => c + 1);
                    finalStandings.AddOrUpdate(Tuple.Create(runUniverse.Get3rdPlaceGameResult().Winner, 3), 1, (k, c) => c + 1);
                    finalStandings.AddOrUpdate(Tuple.Create(runUniverse.Get3rdPlaceGameResult().Loser, 4), 1, (k, c) => c + 1);
                    finalStandings.AddOrUpdate(Tuple.Create(runUniverse.Get5thPlaceGameResult().Winner, 5), 1, (k, c) => c + 1);
                    finalStandings.AddOrUpdate(Tuple.Create(runUniverse.Get5thPlaceGameResult().Loser, 6), 1, (k, c) => c + 1);
                    finalStandings.AddOrUpdate(Tuple.Create(runUniverse.Get7thPlaceGameResult().Winner, 7), 1, (k, c) => c + 1);
                    finalStandings.AddOrUpdate(Tuple.Create(runUniverse.Get7thPlaceGameResult().Loser, 8), 1, (k, c) => c + 1);
                    finalStandings.AddOrUpdate(Tuple.Create(runUniverse.Get9thPlaceGameResult().Winner, 9), 1, (k, c) => c + 1);
                    finalStandings.AddOrUpdate(Tuple.Create(runUniverse.Get9thPlaceGameResult().Loser, 10), 1, (k, c) => c + 1);
                    finalStandings.AddOrUpdate(Tuple.Create(runUniverse.Get11thPlaceGameResult().Winner, 11), 1, (k, c) => c + 1);
                    finalStandings.AddOrUpdate(Tuple.Create(runUniverse.Get11thPlaceGameResult().Loser, 12), 1, (k, c) => c + 1);
                }
            });

            Console.WriteLine("\nFinal Results:\n");
            printProgress(trials);
        }

        public void PredictWinner()
        {
            var universe = new Universe();
            StartSeason(universe);
            FinishSeason(universe);
            Console.WriteLine(universe.GetChampionshipResult().Winner.Owner);
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
            PredictTeamsForWeek(universe, universe.GetTeams(), SeasonWeek.QuarterFinalWeek);
        }

        private void PredictSemifinals(Universe universe)
        {
            PredictTeamsForWeek(universe, universe.GetTeams(), SeasonWeek.SemifinalWeek);
        }

        private void PredictChampionship(Universe universe)
        {
            PredictTeamsForWeek(universe, universe.GetTeams(), SeasonWeek.ChampionshipWeek);
        }
    }
}
