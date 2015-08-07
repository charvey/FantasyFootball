using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyFootball.Service.Fantasy
{
    public class League
    {
        public string Id;
        public string Name;
        public IEnumerable<Team> Teams;
    }
}
