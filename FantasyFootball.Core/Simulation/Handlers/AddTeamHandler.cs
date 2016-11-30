using FantasyFootball.Core.Simulation.Facts;
using FantasyFootball.Core.Simulation.Projections;

namespace FantasyFootball.Core.Simulation.Handlers
{
    class AddTeamHandler : Handler<AddTeam>
    {
        public override void Handle(Universe universe, AddTeam fact)
        {
            TeamProjection.AddTeam(universe, fact.Team);
        }
    }
}
