using FantasyFootball.Core.Objects;

namespace FantasyFootball.Terminal.Draft
{
    public class DraftTeam : Team
    {
        public IEnumerable<Player> Players { get; set; } = Enumerable.Empty<Player>();

        public DraftTeam() { }

        public DraftTeam(Team team)
        {
            this.Id = team.Id;
            this.Name = team.Name;
            this.Owner = team.Owner;
        }
    }
}
