using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Data;

namespace FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.HistoricalDataFilters
{
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
