namespace FantasyFootball.Core.Objects
{
    public class Matchup
    {
        public Team TeamA { get; set; }
        public Team TeamB { get; set; }
        public int Week { get; set; }
    }

    public class MatchupResult : IEquatable<MatchupResult>
    {
        public Team Winner { get; set; }
        public Team Loser { get; set; }
        public bool Tied { get; set; } = false;

        public bool Equals(MatchupResult other)
        {
            return this.Winner == other.Winner
                && this.Loser == other.Loser
                && this.Tied == other.Tied;
        }
    }
}
