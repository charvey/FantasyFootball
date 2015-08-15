using System.Collections.Generic;

namespace FantasyFootball.Web.Models
{
    public class DraftViewModel
    {
        public IEnumerable<string> Teams;
        public IEnumerable<IDictionary<string, Player>> Rounds;
    }
    
    public class Player
    {
        public string Name;
    }
}
