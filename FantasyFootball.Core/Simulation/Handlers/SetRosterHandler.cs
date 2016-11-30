using FantasyFootball.Core.Simulation.Facts;
using FantasyFootball.Core.Simulation.Projections;

namespace FantasyFootball.Core.Simulation.Handlers
{
    class SetRosterHandler : Handler<SetRoster>
    {
        public override void Handle(Universe universe, SetRoster fact)
        {
            RosterProjection.SetRoster(universe, fact.Team, fact.Week, fact.Players);
        }
    }
}
