using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling;
using FantasyFootball.Core.Modeling.RosterModelers;
using FantasyFootball.Core.Modeling.ScoreModelers;
using FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.FooModels;
using FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.HistoricalDataFilters;
using FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.HistoricalGroupSpecifiers;
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
    public class CandidateScoreProvider
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
        private int CurrentWeek = SeasonWeek.Current;
        private readonly FantasySportsService service;
        private readonly CandidateScoreProvider scoreProvider;

        public WinnerPredicter(FantasySportsService service)
        {
            this.service = service;
            var candidate = new ComplexScoreCandidate(
                new PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter(1, 0.1),
                new ByPlayerHistoricalGroupSpecifier(),
                new RawScoreModel());
            scoreProvider = new CandidateScoreProvider(candidate);
        }

        public void PredictWinners(string league_key)
        {
            const int trials = 10000;
            var universe = new Universe();
            StartSeason(universe, league_key);

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
                FinishSeason(runUniverse, league_key);
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

        public void PredictWinner(string league_key)
        {
            var universe = new Universe();
            StartSeason(universe, league_key);
            FinishSeason(universe, league_key);
            Console.WriteLine(universe.GetChampionshipResult().Winner.Owner);
        }

        private ConcurrentDictionary<Tuple<int, int>, Player[]> pastPlayers = new ConcurrentDictionary<Tuple<int, int>, Player[]>();
        private Player[] GetPastPlayers(string league_key, Team team, int week)
        {
            var key = Tuple.Create(team.Id, week);
            return pastPlayers.GetOrAdd(key, _ => service.TeamRoster($"{league_key}.t.{team.Id}", week).players
                .Where(p => p.selected_position.position != "BN")
                .Select(Players.From).ToArray());
        }

        private ConcurrentDictionary<Tuple<int, int>, Player[]> futurePlayers = new ConcurrentDictionary<Tuple<int, int>, Player[]>();
        private Player[] GetFuturePlayers(string league_key, Team team, int week)
        {
            var key = Tuple.Create(team.Id, week);
            return futurePlayers.GetOrAdd(key, _ => service.TeamRoster($"{league_key}.t.{team.Id}", week).players
                .Select(Players.From).ToArray());
        }

        private void StartSeason(Universe universe, string league_key)
        {
            foreach (var team in service.Teams(league_key).Select(Teams.From))
                universe.AddFact(new AddTeam { Team = team });

            for (int week = 1; week < CurrentWeek; week++)
            {
                Console.WriteLine($"Recording Week #{week}");
                RecordWeek(universe, league_key, week);
            }
        }

        private void FinishSeason(Universe universe, string league_key)
        {
            for (int week = CurrentWeek; week <= SeasonWeek.RegularSeasonEnd; week++)
            {
                Console.WriteLine($"Predicting Week #{week}");
                PredictWeek(universe, league_key, week);
            }

            Console.WriteLine($"Predicting Quarterfinals");
            PredictQuarterfinals(universe, league_key);
            Console.WriteLine($"Predicting Semifinals");
            PredictSemifinals(universe, league_key);
            Console.WriteLine($"Predicting Championship");
            PredictChampionship(universe, league_key);
        }

        private void RecordWeek(Universe universe, string league_key, int week)
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

            foreach (var matchup in Matchups.GetByWeek(service, league_key, week))
                universe.AddFact(new AddMatchup { Matchup = matchup });

            foreach (var team in universe.GetTeams())
            {
                universe.AddFact(new SetRoster
                {
                    Team = team,
                    Week = week,
                    Players = GetPastPlayers(league_key, team, week)
                });
            }
        }

        private void PredictWeek(Universe universe, string league_key, int week)
        {
            foreach (var matchup in Matchups.GetByWeek(service, league_key, week))
                universe.AddFact(new AddMatchup { Matchup = matchup });

            PredictTeamsForWeek(universe, league_key, universe.GetTeams(), week);
        }

        private void PredictTeamsForWeek(Universe universe, string league_key, IEnumerable<Team> teams, int week)
        {
            foreach (var team in teams)
            {
                var allPlayers = GetFuturePlayers(league_key, team, week);
                var roster = new MostLikelyScoreRosterModeler(new RealityScoreModeler(DumpData.GetScore))
                    .Model(new RosterSituation(allPlayers, week)).Outcomes.Single();

                foreach (var player in roster.Players)
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
                    Players = roster.Players
                });
            }
        }

        private void PredictQuarterfinals(Universe universe, string league_key)
        {
            PredictTeamsForWeek(universe, league_key, universe.GetTeams(), SeasonWeek.QuarterFinalWeek);
        }

        private void PredictSemifinals(Universe universe, string league_key)
        {
            PredictTeamsForWeek(universe, league_key, universe.GetTeams(), SeasonWeek.SemifinalWeek);
        }

        private void PredictChampionship(Universe universe, string league_key)
        {
            PredictTeamsForWeek(universe, league_key, universe.GetTeams(), SeasonWeek.ChampionshipWeek);
        }
    }
}
