using System.Xml.Serialization;

namespace FantasyFootball.Data.Yahoo.Models
{
    public class LeagueScoreboard
    {
        public int week;
        public Matchup[] matchups;
    }

    [XmlType("matchup")]
    public class Matchup
    {
        public int week;
        public Team[] teams;
    }
}
