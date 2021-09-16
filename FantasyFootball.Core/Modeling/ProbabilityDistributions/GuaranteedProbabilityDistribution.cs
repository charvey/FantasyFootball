namespace FantasyFootball.Core.Modeling.ProbabilityDistributions
{
    public class GuaranteedProbabilityDistribution<T> : ProbabilityDistribution<T> where T : IEquatable<T>
    {
        private readonly T outcome;

        public GuaranteedProbabilityDistribution(T outcome)
        {
            this.outcome = outcome;
        }

        public IEnumerable<T> Outcomes
        {
            get { return new[] { outcome }; }
        }

        public double Probability(T outcome)
        {
            if (this.outcome.Equals(outcome)) return 1;
            else return 0;
        }
    }
}
