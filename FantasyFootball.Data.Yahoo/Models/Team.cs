using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyFootball.Data.Yahoo.Models
{
    public class Team
    {
        public string team_key;
        public string team_id;
        public string name;
        public string url;
        public TeamLogos team_logos;
    }

    public class TeamLogos
    {
        public TeamLogo team_logo;
    }

    public class TeamLogo
    {
        public string size;
        public string url;
    }
}
