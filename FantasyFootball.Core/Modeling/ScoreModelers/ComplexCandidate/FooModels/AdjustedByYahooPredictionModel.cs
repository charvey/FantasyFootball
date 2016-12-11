using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Data;
using System.Linq;

namespace FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.FooModels
{
    public class AdjustedByYahooPredictionModel : FooModel
    {
        public string Name => "Adjusted by Yahoo Prediction";

        public bool CanPredict(Situation situation)
        {
            return DumpData.GetPrediction(situation.Player, situation.Week, situation.Week).HasValue;
        }

        public double GetHistoricalValue(Situation s)
        {
            var actual = DumpData.GetActualScore(s.Player, s.Week).Value;
            var prediction = DumpData.GetPrediction(s.Player, s.Week, s.Week).Value;

            return (actual + 10) / (prediction + 10);
        }

        public double[] GetSituationValues(double[] historicalData, Situation situation)
        {
            var prediction = DumpData.GetPrediction(situation.Player, situation.Week, situation.Week).Value;

            return historicalData.Select(x => x * (prediction + 10) - 10).ToArray();
        }
    }
}
