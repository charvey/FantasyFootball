﻿using System.Collections.Generic;

namespace FantasyFootball.Service.Fantasy
{
    public class League
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Team> Teams { get; set; }
        public IEnumerable<Player> Players { get; set; }
        public ISet<DraftPick> DraftPicks { get; set; }
    }
}
