using System.Xml.Serialization;

namespace FantasyFootball.Data.Yahoo.Models
{
    public class FantasyContent
    {
        public Game[] game;
        public League[] league;
        public Player[] player;
        public Team[] team;
    }

    [XmlType("fantasy_content")]
    public class FantasyContentXml
    {
        public Game game;
        public League league;
        public Player player;
        public Team team;
    }
}
