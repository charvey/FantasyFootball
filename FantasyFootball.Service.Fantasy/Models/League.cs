using System.Collections.Generic;

namespace FantasyFootball.Service.Fantasy.Models
{
    public class League
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
        public virtual ICollection<Player> Players { get; set; }
        public virtual ICollection<DraftPick> DraftPicks { get; set; }
        public virtual ICollection<RosterPosition> RosterPositions { get; set; }
    }
}
