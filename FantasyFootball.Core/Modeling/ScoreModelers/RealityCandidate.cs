using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Data;

namespace FantasyFootball.Core.Modeling.ScoreModelers
{
    public class RealityCandidate : Candidate
    {
        public RealityCandidate()
        {
            Name = "Reality";
            GetFunction = s => new ConstantProbabilityDistribution(DumpData.GetActualScore(s.Player, s.Week).Value);
            CanBeTestedOn = s => DumpData.GetActualScore(s.Player, s.Week).HasValue;
        }
    }
}
