namespace ClickyDraft
{
    public class League
    {
        public string DisplayName;
        public FantasyTeam[] FantasyTeams;
        public LeagueUser[] LeagueUsers;
    }

    public class LeagueUser
    {
        public int Id;
        public string UserName;
        public string DisplayName;
    }

    public class FantasyTeam
    {
        public int Id;
        public int LeagueUserId;
        public string TeamName;
        public int DraftPosition;
    }
}
