using Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models
{
    public class LeagueRankingsViewModel:LeagueViewModel
    {
        public IEnumerable<Ranking> Rankings;
    }
}