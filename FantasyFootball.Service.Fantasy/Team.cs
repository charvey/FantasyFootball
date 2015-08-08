using System.Collections.Generic;

namespace FantasyFootball.Service.Fantasy
{
    public class Team
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public League League { get; set; }
        public IEnumerable<Player> Players { get; set; }
    }
}
