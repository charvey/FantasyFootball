using FantasyFootball.Core.Simulation.Facts;
using FantasyFootball.Core.Simulation.Projections;

namespace FantasyFootball.Core.Simulation.Handlers
{
    class AddMatchupHandler : Handler<AddMatchup>
    {
        public override void Handle(Universe universe, AddMatchup fact)
        {
            MatchupProjection.AddMatchup(universe, fact.Matchup);
        }
    }
}
