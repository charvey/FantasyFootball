using FantasyFootball.Core.Analysis;

namespace FantasyFootball.Core.Modeling.ScoreModelers
{
    public class NoScoreCandidate : Candidate
    {
        public NoScoreCandidate()
        {
            this.Name = "No Score";
            this.GetFunction = s => new ConstantProbabilityDistribution(0);
            this.CanBeTestedOn = s => true;
        }
    }
}
