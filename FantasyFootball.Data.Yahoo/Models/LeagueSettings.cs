using System.Xml.Serialization;

namespace FantasyFootball.Data.Yahoo.Models
{
    public class LeagueSettings
    {
        public int playoff_start_week;
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
