using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling.ProbabilityDistributions;

namespace FantasyFootball.Core.Modeling.ScoreModelers
{
    class RealityScoreModeler : ScoreModeler
    {
        public ProbabilityDistribution<double> Model(ScoreSituation situation)
        {
            return new GuaranteedProbabilityDistribution<double>(DumpData.GetScore(situation.Player, situation.Week));
        }
    }
}
