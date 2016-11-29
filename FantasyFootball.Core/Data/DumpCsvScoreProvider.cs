using FantasyFootball.Core.Objects;

namespace FantasyFootball.Core.Data
{
    public class DumpCsvScoreProvider : ScoreProvider
    {
        public double GetScore(Player player, int week)
        {
            return DumpData.GetScore(player, week);
        }
    }
}
