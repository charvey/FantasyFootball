using FantasyFootball.Core.Objects;

namespace FantasyFootball.Core.Modeling
{
    public interface ProbabilityDistribution<T> where T : IEquatable<T>
    {
        double Probability(T outcome);
        IEnumerable<T> Outcomes { get; }
    }

    public interface MatchupModeler : Modeler
    {
        ProbabilityDistribution<MatchupResult> Model(Matchup matchup);
    }
}
