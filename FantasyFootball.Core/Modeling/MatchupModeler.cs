using FantasyFootball.Core.Objects;
using System;
using System.Collections.Generic;

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
