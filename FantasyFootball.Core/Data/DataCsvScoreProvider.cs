using FantasyFootball.Core.Objects;

namespace FantasyFootball.Core.Data
{
    public class DataCsvScoreProvider : ScoreProvider
    {
        public double GetScore(Player player, int week)
        {
            return Scores.GetScore(player, week);
        }
    }
}
