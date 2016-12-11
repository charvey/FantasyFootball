using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Data;

namespace FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.HistoricalDataFilters
{
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
}
