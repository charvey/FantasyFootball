﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyFootball.Service.Fantasy
{
    public class League
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public IEnumerable<Team> Teams { get; set; }
    }
}
