using FantasyFootball.Core.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyFootball.Core.Modeling
{
    public interface ProbabilityDistribution<T>
    {
        double Probability(T outcome);
    }

    public interface MatchupModeler
    {
        ProbabilityDistribution<MatchupResult> Model(Matchup matchup);
    }
}
