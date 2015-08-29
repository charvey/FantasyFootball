using System;

namespace FantasyFootball.Service.Fantasy.Models
{
    public class PlayerPosition
    {
        public Guid Id { get; set; }
        public virtual LeaguePlayer Player { get; set; }
        public virtual RosterPosition Position { get; set; }
    }
}
