using System.Collections.Generic;

namespace FantasyFootball.Service.Fantasy.Models
{
    public class League
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Team> Teams { get; set; }
        public ICollection<Player> Players { get; set; }
        public ISet<DraftPick> DraftPicks { get; set; }
    }
}
