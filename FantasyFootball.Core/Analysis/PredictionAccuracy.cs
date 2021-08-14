using FantasyFootball.Core.Data;
using FantasyFootball.Data.Yahoo;
using MathNet.Numerics.Distributions;
using MathNet.Numerics.Statistics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Yahoo;

namespace FantasyFootball.Core.Analysis
{
    public class PredictionAccuracy
    {
        private readonly FantasySportsService fantasySportsService;
        private readonly IFullPredictionRepository predictionRepository;
        private readonly TextReader textReader;
        private readonly TextWriter textWriter;

        public PredictionAccuracy(FantasySportsService fantasySportsService, IFullPredictionRepository fullPredictionRepository, TextReader textReader, TextWriter textWriter)
        {
            this.fantasySportsService = fantasySportsService;
            this.predictionRepository = fullPredictionRepository;
            this.textReader = textReader;
            this.textWriter = textWriter;
        }

        public void Do(LeagueKey leagueKey)
        {
            var league = fantasySportsService.League(leagueKey);

            var weekStarts = new Dictionary<int, DateTime>();
            weekStarts[league.start_week] = DateTime.Parse(league.start_date);
            for (var week = 2; week <= league.end_week; week++)
                weekStarts[week] = weekStarts[week - 1].AddDays(7);
            Debug.Assert(weekStarts[league.end_week] < DateTime.Parse(league.end_date));

            var predictions = predictionRepository.GetAll(leagueKey)
                .Where(p => p.Week <= league.end_week)
                .Where(p => p.AsOf < weekStarts[p.Week])
                .Where(p => p.Value > 0)
                .ToList();

            textWriter.WriteLine($"Total predictions: {predictions.Count}");

            var points = Enumerable.Range(1, league.end_week)
                .ToDictionary(
                    w => w,
                    w => fantasySportsService.LeaguePlayersWeekStats(leagueKey, w)
                        .ToDictionary(p => p.player_id.ToString(), p => p.player_points.total)
                );

            textWriter.WriteLine($"Total points: {points.Count}");

            File.WriteAllLines("pred_accu.csv", new[] { "Player,Week,Time Delta,Prediction,Actual" }
            .Concat(predictions.Select(prediction =>
            {
                var time = (weekStarts[prediction.Week] - prediction.AsOf).TotalDays;
                var actual = points[prediction.Week][prediction.PlayerId];
                return $"{prediction.PlayerId},{prediction.Week},{time:F2},{prediction.Value:F2},{actual:F2}";
            })));

            var mv = Statistics.MeanVariance(predictions.Select(p => points[p.Week][p.PlayerId] - p.Value));
            textWriter.WriteLine($"{mv.Item1} {mv.Item2}");
            textWriter.WriteLine(new Normal(mv.Item1, Math.Sqrt(mv.Item2)));
        }
    }
}
