using System.Xml.Serialization;

namespace FantasyFootball.Data.Yahoo.Models
{
    [XmlType("fantasy_content")]
    public class FantasyContent
    {
        public Game[] game;
        public League[] league;
        public Player[] player;
        public Team team;
    }
}
