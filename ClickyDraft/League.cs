namespace ClickyDraft
{
    internal class League
    {
        public FantasyTeam[] FantasyTeams;
        public LeagueUser[] LeagueUsers;
    }

    internal class LeagueUser
    {
        public int Id;
        public string UserName;
        public string DisplayName;
    }

    internal class FantasyTeam
    {
        public int Id;
        public int LeagueUserId;
        public string TeamName;
        public int DraftPosition;
    }
}
