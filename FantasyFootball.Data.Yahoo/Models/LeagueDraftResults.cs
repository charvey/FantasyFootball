using System.Xml.Serialization;

namespace FantasyFootball.Data.Yahoo.Models
{
    [XmlType("draft_result")]
    public class DraftResult
    {
        public int pick;
        public int round;
        public string team_key;
        public string player_key;
    }
}
