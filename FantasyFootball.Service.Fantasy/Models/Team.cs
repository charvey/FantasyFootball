using System.Collections.Generic;

namespace FantasyFootball.Service.Fantasy.Models
{
    public class Team
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public virtual League League { get; set; }
        public virtual ICollection<LeaguePlayer> Players { get; set; }
    }
}
