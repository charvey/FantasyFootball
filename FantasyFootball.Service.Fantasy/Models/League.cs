using System.Collections.Generic;

namespace FantasyFootball.Service.Fantasy.Models
{
    public class League
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ICollection<Team> Teams { get; set; }
        public ICollection<Player> Players { get; set; }
        public ICollection<DraftPick> DraftPicks { get; set; }
    }
}
