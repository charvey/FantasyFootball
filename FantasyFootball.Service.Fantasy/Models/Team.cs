using System.Collections.Generic;

namespace FantasyFootball.Service.Fantasy.Models
{
    public class Team
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public League League { get; set; }
        public ICollection<Player> Players { get; set; }
    }
}
