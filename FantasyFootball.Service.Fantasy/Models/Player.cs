using System.Collections.Generic;

namespace FantasyFootball.Service.Fantasy.Models
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<League> Leagues { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
    }
}
