
using System.Xml.Serialization;

namespace FantasyFootball.Data.Yahoo.Models
{
    public class Game
    {
        public string game_key;
        public string game_id;
        public string name;
        public string code;
        public string type;
        public string url;
        public int season;
        public StatCategories stat_categories;
    }

    public class StatCategories
    {
        public StatCategory[] stats;
    }

    [XmlType("stat")]
    public class StatCategory
    {
        public int stat_id;
        public string name;
        public string display_name;
    }
}
