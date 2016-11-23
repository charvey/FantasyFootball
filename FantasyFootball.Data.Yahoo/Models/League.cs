
namespace FantasyFootball.Data.Yahoo.Models
{
    public class League
    {
        public string league_key;
        public string league_id;
        public string name;
        public string url;
        public string draft_status;
        public int num_teams;
        public int season;
        public LeagueSettings settings;
        public LeagueScoreboard scoreboard;
    }
}
