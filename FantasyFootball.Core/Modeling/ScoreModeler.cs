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

    public interface ScoreModeler
    {
        ProbabilityDistribution<double> Model(ScoreSituation situation);
    }
}
