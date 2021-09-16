using FantasyFootball.Core.Objects;

namespace FantasyFootball.Core.Simulation.Projections
{
    public class TeamProjection : Projection<List<Team>>
    {
        public static Team[] GetTeams(Universe universe)
        {
            return GetState(universe).ToArray();
        }

        public static void AddTeam(Universe universe, Team team)
        {
            GetState(universe).Add(team);
        }

        protected override List<Team> Clone(List<Team> original)
        {
            return new List<Team>(original);
        }
    }
}