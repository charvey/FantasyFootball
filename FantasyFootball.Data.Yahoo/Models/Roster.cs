using System.Xml.Serialization;

namespace FantasyFootball.Data.Yahoo.Models
{
    [XmlType("roster")]
    public class Roster
    {
        public Player[] players;
    }
}