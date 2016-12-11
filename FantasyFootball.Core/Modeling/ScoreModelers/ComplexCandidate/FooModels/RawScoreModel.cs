using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Data;

namespace FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.FooModels
{
    public class RawScoreModel : FooModel
    {
        public string Name => "Raw Scores";

        public bool CanPredict(Situation situation)
        {
            return true;
        }

        public double GetHistoricalValue(Situation s)
        {
            return DumpData.GetActualScore(s.Player, s.Week).Value;
        }

        public double[] GetSituationValues(double[] historicalData, Situation situation)
        {
            return historicalData;
        }
    }
}
