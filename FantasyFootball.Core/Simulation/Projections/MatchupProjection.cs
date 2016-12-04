using FantasyFootball.Core.Objects;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Core.Simulation.Projections
{
    class MatchupProjection : Projection<List<Matchup>>
    {
        public static Matchup[] GetMatchups(Universe universe, int week)
        {
            return GetState(universe).Where(m => m.Week == week).ToArray();
        }

        public static void AddMatchup(Universe universe, Matchup matchup)
        {
            GetState(universe).Add(matchup);
        }

        protected override List<Matchup> Clone(List<Matchup> original)
        {
            return new List<Matchup>(original);
        }
    }
}
