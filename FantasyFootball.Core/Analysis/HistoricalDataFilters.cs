using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyFootball.Core.Analysis
{
    public interface HistoricalDataFilter
    {
        string Name { get; }
        bool Include(Situation situation);
    }

    public class AllHistoricalDataFilter : HistoricalDataFilter
    {
        public string Name => "All Historical";
        public bool Include(Situation situation) => true;
    }

    public class PredictedToScoreAtLeastHistoricalDataFilter : HistoricalDataFilter
    {
        private readonly double predictionMinimum;

        public PredictedToScoreAtLeastHistoricalDataFilter(double predictionMinimum)
        {
            this.predictionMinimum = predictionMinimum;
        }

        public string Name => $"Predicted at least {predictionMinimum}";
        public bool Include(Situation situation)
        {
            var prediction = DumpData.GetPrediction(situation.Player, situation.Week, situation.Week);
            return prediction.HasValue && prediction.Value >= predictionMinimum;
        }
    }

    public class PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter : HistoricalDataFilter
    {
        private readonly double predictionMinimum;
        private readonly double relativeActualMinimum;

        public PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter(double predictionMinimum, double relativeActualMinimum)
        {
            this.predictionMinimum = predictionMinimum;
            this.relativeActualMinimum = relativeActualMinimum;
        }

        public string Name => $"Predicted at least {predictionMinimum} and scored at least {relativeActualMinimum:P} of prediction";
        public bool Include(Situation situation)
        {
            var prediction = DumpData.GetPrediction(situation.Player, situation.Week, situation.Week);
            var actual = DumpData.GetActualScore(situation.Player, situation.Week).Value;

            return prediction.HasValue
                && prediction.Value >= predictionMinimum
                && actual >= prediction.Value * relativeActualMinimum;
        }
    }
}
