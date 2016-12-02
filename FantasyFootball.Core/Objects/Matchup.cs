namespace FantasyFootball.Core.Objects
{
    public class Matchup
    {
        public Team TeamA { get; set; }
        public Team TeamB { get; set; }
        public int Week { get; set; }
    }

    public class MatchupResult
    {
        public Team Winner { get; set; }
        public Team Loser { get; set; }
        public bool Tied { get; set; } = false;
    }
}
