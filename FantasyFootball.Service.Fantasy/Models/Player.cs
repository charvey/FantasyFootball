using System.Collections.Generic;

namespace FantasyFootball.Service.Fantasy.Models
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public ICollection<League> Leagues { get; set; }
        public ICollection<Team> Teams { get; set; }
    }
}
