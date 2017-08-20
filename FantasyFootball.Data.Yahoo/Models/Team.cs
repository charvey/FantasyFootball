using System.Xml.Serialization;

namespace FantasyFootball.Data.Yahoo.Models
{
    [XmlType("team")]
    public class Team
    {
        public string team_key;
        public string team_id;
        public string name;
        public string url;
        public TeamLogos team_logos;
        public Manager[] managers;
        public Roster roster;
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

    [XmlType("manager")]
    public class Manager
    {
        public string nickname;
        public string guid;
    }
}
