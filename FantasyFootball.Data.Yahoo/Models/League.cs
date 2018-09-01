
namespace FantasyFootball.Data.Yahoo.Models
{
    public class League
    {
        public string league_key;
        public int league_id;
        public string name;
        public string url;
        public string draft_status;
        public int num_teams;
        public int current_week;
        public int start_week;
        public int end_week;
        public string game_code;
        public int season;
        public DraftResult[] draft_results;
        public LeagueSettings settings;
        public LeagueScoreboard scoreboard;
        public Transaction[] transactions;
    }
}
