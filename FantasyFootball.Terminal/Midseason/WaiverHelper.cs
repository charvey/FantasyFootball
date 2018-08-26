using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling;
using FantasyFootball.Core.Modeling.RosterModelers;
using FantasyFootball.Core.Modeling.ScoreModelers;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Terminal.Midseason
{
    public class WaiverHelper
    {
        public void Help(FantasySportsService service, IPredictionRepository predictionRepository, TextWriter output, string league_key, int team_id)
        {
            var league = service.League(league_key);
            var all = service.LeaguePlayers(league_key).Select(Players.From).ToList();
            var available = service.LeaguePlayers(league_key, status: "A").Select(Players.From).ToList();
            var mine = service.TeamRoster($"{league_key}.t.{team_id}").players.Select(Players.From).ToList();

            var baseScore = GetRemainingScore(predictionRepository, mine, league);

            var results = new ConcurrentBag<Tuple<Player, Player, double>>();
            mine.ForEach(t =>
            {
                Console.WriteLine($"Evaluating losing {t.Name}");
                available.ForEach(a =>
                {
                    var newRoster = mine.ToList();
                    newRoster.Remove(t);
                    newRoster.Add(a);
                    var score = GetRemainingScore(predictionRepository, newRoster, league);
                    if (score > baseScore)
                        results.Add(Tuple.Create(t, a, score - baseScore));
                });
                Console.WriteLine($"Done evaluating losing {t.Name}");
            });
            var top100 = results.OrderByDescending(x => x.Item3).Take(100).ToList();
            foreach (var result in top100)
            {
                output.WriteLine($"By dropping {result.Item1.Name} and adding {result.Item2.Name} you will gain {result.Item3:#0.00}");
            }
        }

        private double GetRemainingScore(IPredictionRepository predictionRepository, IEnumerable<Player> players, Data.Yahoo.Models.League league)
        {
            return Enumerable.Range(1, league.end_week)
                .Where(w => w >= league.current_week)
                .Select(w => GetWeekScore(predictionRepository, players, league.season, w)).Sum();
        }

        private readonly ConcurrentDictionary<Tuple<string, int>, double> predictions = new ConcurrentDictionary<Tuple<string, int>, double>();

        private double GetWeekScore(IPredictionRepository predictionRepository, IEnumerable<Player> players, int year, int week)
        {
            return new MostLikelyScoreRosterModeler(new RealityScoreModeler((p, w) => predictions.GetOrAdd(Tuple.Create(p.Id, week), t => predictionRepository.GetPrediction(t.Item1, year, t.Item2))))
                .Model(new RosterSituation(players.ToArray(), week))
                .Outcomes.Single().Players
                .Sum(p => predictions.GetOrAdd(Tuple.Create(p.Id, week), t => predictionRepository.GetPrediction(t.Item1, year, t.Item2)));
        }
    }
}
