using FantasyFootball.Core.Objects;

namespace FantasyFootball.Core.Simulation.Facts
{
    public class SetScore : Fact
    {
        public Player Player { get; set; }
        public int Week { get; set; }
        public double Score { get; set; }
    }
}
