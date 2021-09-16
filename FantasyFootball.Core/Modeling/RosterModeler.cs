using FantasyFootball.Core.Objects;

namespace FantasyFootball.Core.Modeling
{
    public struct RosterSituation
    {
        public Player[] Players { get; private set; }
        public int Week { get; private set; }

        public RosterSituation(Player[] players, int week)
        {
            this.Players = players;
            this.Week = week;
        }
    }

    public struct Roster : IEquatable<Roster>
    {
        public Player[] Players { get; private set; }

        public Roster(Player[] players)
        {
            this.Players = players;
        }

        public bool Equals(Roster other)
        {
            Func<Roster, string> hash = r => string.Join("|", r.Players.OrderBy(p => p.Id).Select(p => p.Id));
            return hash(this) == hash(other);
        }
    }

    public interface RosterModeler
    {
        ProbabilityDistribution<Roster> Model(RosterSituation roster);
    }
}
