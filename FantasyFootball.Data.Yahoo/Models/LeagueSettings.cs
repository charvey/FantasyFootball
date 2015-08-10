using System.Xml.Serialization;

namespace FantasyFootball.Data.Yahoo.Models
{
    public class LeagueSettings
    {
        public RosterPosition[] roster_positions;
    }

    [XmlType("roster_position")]
    public class RosterPosition
    {
        public string position;
        public string position_type;
        public int count;
    }
}
