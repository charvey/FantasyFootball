using FantasyFootball.Core.Objects;

namespace FantasyFootball.Core.Modeling
{
    public struct ScoreSituation
    {
        public Player Player { get; private set; }
        public int Week { get; private set; }

        public ScoreSituation(Player player, int week)
        {
            this.Player = player;
            this.Week = week;
        }
    }

    public interface ContinuousProbabilityDistribution<T>
    {
        double Get(T left, T right);
    }

    public interface ScoreModeler
    {
        ContinuousProbabilityDistribution<double> Model(ScoreSituation situation);
    }
}
