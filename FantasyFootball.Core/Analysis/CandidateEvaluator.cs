namespace FantasyFootball.Core.Analysis
{
    public abstract class CandidateEvaluator
    {
        public void EvaluateAll(TextWriter @out, IEnumerable<Candidate> candidates)
        {
            foreach (var candidate in candidates)
            {
                @out.WriteLine(candidate.Name + "\t" + Evaluate(candidate));
            }
        }

        public Candidate FindBest(IEnumerable<Candidate> candidates)
        {
            return candidates.OrderByDescending(Evaluate).First();
        }

        public abstract double Evaluate(Candidate candidate);
    }
}
