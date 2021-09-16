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
using System.Collections.Concurrent;
using System.Diagnostics;
using Yahoo;

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
        private readonly FantasySportsService service;
        private readonly ILatestPredictionRepository predictionRepository;
        private readonly CandidateScoreProvider scoreProvider;
        private readonly TextWriter writer;

        public WinnerPredicter(FantasySportsService service, ILatestPredictionRepository predictionRepository, TextWriter writer)
        {
            this.service = service;
            var candidate = new ComplexScoreCandidate(
                new PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter(1, 0.1),
                new ByPlayerHistoricalGroupSpecifier(),
                new RawScoreModel());
            scoreProvider = new CandidateScoreProvider(candidate);
            this.predictionRepository = predictionRepository;
            this.writer = writer;
        }

        public void PredictWinners(LeagueKey leagueKey)
        {
            const int trials = 10000;
            var universe = new Universe();
            StartSeason(universe, leagueKey);

            var stopwatch = Stopwatch.StartNew();
            var winners = new ConcurrentDictionary<Team, int>();
            var playoffAppearances = new ConcurrentDictionary<Team, int>();
            var finalStandings = new ConcurrentDictionary<Tuple<Team, int>, int>();

            Action<int> printProgress = (int t) =>
              {
                  var average = TimeSpan.FromMilliseconds(stopwatch.ElapsedMilliseconds / t);
                  writer.WriteLine($"\n{t} trials ran in {stopwatch.Elapsed} (average {average} each)");
                  writer.WriteLine("\nChampionships Wins");
                  foreach (var team in winners.OrderByDescending(x => x.Value))
                      writer.WriteLine($"{team.Key.Owner} {team.Value} {1.0 * team.Value / t:P}");
                  writer.WriteLine("\nPlayoff Appearances");
                  foreach (var team in playoffAppearances.OrderByDescending(x => x.Value))
                      writer.WriteLine($"{team.Key.Owner} {team.Value} {1.0 * team.Value / t:P}");
                  writer.WriteLine("\nFinal Rankings");
                  foreach (var team in universe.GetTeams().OrderBy(team => team.Owner))
                  {
                      writer.Write(team.Owner);
                      foreach (var standing in Enumerable.Range(1, 12))
                      {
                          writer.Write($",{1.0 * finalStandings.GetOrAdd(Tuple.Create(team, standing), 0) / t:P}");
                      }
                      writer.WriteLine();
                  }
              };

            Enumerable.Range(1, trials).AsParallel().ForAll(_ =>
            {
                writer.WriteLine($"Starting Trial #{_}");
                var runUniverse = universe.Clone();
                FinishSeason(runUniverse, leagueKey);
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

            writer.WriteLine("\nFinal Results:\n");
            printProgress(trials);
        }

        public void PredictWinner(LeagueKey leagueKey)
        {
            var universe = new Universe();
            StartSeason(universe, leagueKey);
            FinishSeason(universe, leagueKey);
            writer.WriteLine(universe.GetChampionshipResult().Winner.Owner);
        }

        private ConcurrentDictionary<Tuple<int, int>, Player[]> pastPlayers = new ConcurrentDictionary<Tuple<int, int>, Player[]>();
        private Player[] GetPastPlayers(LeagueKey leagueKey, Team team, int week)
        {
            var key = Tuple.Create(team.Id, week);
            return pastPlayers.GetOrAdd(key, _ => service.TeamRoster($"{leagueKey}.t.{team.Id}", week).players
                .Where(p => p.selected_position.position != "BN")
                .Select(Players.From).ToArray());
        }

        private ConcurrentDictionary<Tuple<int, int>, Player[]> futurePlayers = new ConcurrentDictionary<Tuple<int, int>, Player[]>();
        private Player[] GetFuturePlayers(LeagueKey leagueKey, Team team, int week)
        {
            var key = Tuple.Create(team.Id, week);
            return futurePlayers.GetOrAdd(key, _ => service.TeamRoster($"{leagueKey}.t.{team.Id}", week).players
                .Select(Players.From).ToArray());
        }

        private void StartSeason(Universe universe, LeagueKey leagueKey)
        {
            foreach (var team in service.Teams(leagueKey).Select(Teams.From))
                universe.AddFact(new AddTeam { Team = team });

            for (int week = 1; week < service.League(leagueKey).current_week; week++)
            {
                writer.WriteLine($"Recording Week #{week}");
                RecordWeek(universe, leagueKey, week);
            }
        }

        private void FinishSeason(Universe universe, LeagueKey leagueKey)
        {
            for (int week = service.League(leagueKey).current_week; week < service.LeagueSettings(leagueKey).playoff_start_week; week++)
            {
                writer.WriteLine($"Predicting Week #{week}");
                PredictWeek(universe, leagueKey, week);
            }

            writer.WriteLine($"Predicting Quarterfinals");
            PredictQuarterfinals(universe, leagueKey);
            writer.WriteLine($"Predicting Semifinals");
            PredictSemifinals(universe, leagueKey);
            writer.WriteLine($"Predicting Championship");
            PredictChampionship(universe, leagueKey);
        }

        private void RecordWeek(Universe universe, LeagueKey leagueKey, int week)
        {
            foreach (var player in Players.All())
            {
                universe.AddFact(new SetScore
                {
                    Player = player,
                    Week = week,
                    Score = predictionRepository.GetPrediction(leagueKey, player.Id, week)
                });
            }

            foreach (var matchup in Matchups.GetByWeek(service, leagueKey, week))
                universe.AddFact(new AddMatchup { Matchup = matchup });

            foreach (var team in universe.GetTeams())
            {
                universe.AddFact(new SetRoster
                {
                    Team = team,
                    Week = week,
                    Players = GetPastPlayers(leagueKey, team, week)
                });
            }
        }

        private void PredictWeek(Universe universe, LeagueKey leagueKey, int week)
        {
            foreach (var matchup in Matchups.GetByWeek(service, leagueKey, week))
                universe.AddFact(new AddMatchup { Matchup = matchup });

            PredictTeamsForWeek(universe, leagueKey, universe.GetTeams(), week);
        }

        private void PredictTeamsForWeek(Universe universe, LeagueKey leagueKey, IEnumerable<Team> teams, int week)
        {
            foreach (var team in teams)
            {
                var allPlayers = GetFuturePlayers(leagueKey, team, week);
                var roster = new MostLikelyScoreRosterModeler(new RealityScoreModeler((p, w) => predictionRepository.GetPrediction(leagueKey, p.Id, week)))
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

        private void PredictQuarterfinals(Universe universe, LeagueKey leagueKey)
        {
            PredictTeamsForWeek(universe, leagueKey, universe.GetTeams(), service.League(leagueKey).end_week - 2);
        }

        private void PredictSemifinals(Universe universe, LeagueKey leagueKey)
        {
            PredictTeamsForWeek(universe, leagueKey, universe.GetTeams(), service.League(leagueKey).end_week - 1);
        }

        private void PredictChampionship(Universe universe, LeagueKey leagueKey)
        {
            PredictTeamsForWeek(universe, leagueKey, universe.GetTeams(), service.League(leagueKey).end_week);
        }
    }
}
