using Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Web.Models
{
    public class LeagueViewModel
    {
        public League League;
    }

    public class LeagueViewModel<T> : LeagueViewModel
    {
        public T PageModel;
    }
}