using FantasyFootball.Core.Objects;

namespace FantasyFootball.Core.Simulation.Facts
{
    public class SetRoster : Fact
    {
        public Team Team { get; set; }
        public int Week { get; set; }
        public Player[] Players { get; set; }
    }
}
