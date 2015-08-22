using System.Collections.Generic;

namespace FantasyFootball.Service.Fantasy.Models
{
    public class Player
    {
        public string Id { get; set; }
        public string PlayerId { get; set; }
        public int ByeWeek { get; set; }
        public virtual League League { get; set; }
        public virtual ICollection<Team> Teams { get; set; }
    }
}
