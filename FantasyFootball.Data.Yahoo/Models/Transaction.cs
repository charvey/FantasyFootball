using System.Xml.Serialization;

namespace FantasyFootball.Data.Yahoo.Models
{
    [XmlType("transaction")]
    public class Transaction
    {
        public string transaction_key;
        public string transaction_id;
        public string type;
        public string status;
        public int timestamp;
        public string trader_team_key;
        public string trader_team_name;
        public string tradee_team_key;
        public string tradee_team_name;
        public Player[] players;
    }
}
