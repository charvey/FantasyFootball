using FantasyFootball.Core.Modeling.ProbabilityDistributions;
using FantasyFootball.Core.Objects;
using System;

namespace FantasyFootball.Core.Modeling.ScoreModelers
{
    public class RealityScoreModeler : ScoreModeler
    {
        private readonly Func<Player, int, double> scores;

        public RealityScoreModeler(Func<Player, int, double> scores)
        {
            this.scores = scores;
        }

        public ProbabilityDistribution<double> Model(ScoreSituation situation)
        {
            return new GuaranteedProbabilityDistribution<double>(scores(situation.Player, situation.Week));
        }
    }
}
