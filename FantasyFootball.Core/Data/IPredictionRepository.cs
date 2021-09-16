using Yahoo;

namespace FantasyFootball.Core.Data
{
    public interface ILatestPredictionRepository
    {
        double GetPrediction(LeagueKey leagueKey, string playerId, int week);
        double[] GetPredictions(LeagueKey leagueKey, string playerId, IEnumerable<int> weeks);
    }

    public interface IFullPredictionRepository
    {
        void AddPrediction(LeagueKey leagueKey, string playerId, int week, double value, DateTime asOf);
        IReadOnlyList<Prediction> GetAll(LeagueKey leagueKey);
        double GetPrediction(LeagueKey leagueKey, string playerId, int week);
        double[] GetPredictions(LeagueKey leagueKey, string playerId, IEnumerable<int> weeks);
    }

    public interface Prediction
    {
        LeagueKey LeagueKey { get; }
        string PlayerId { get; }
        int Week { get; }
        DateTime AsOf { get; }
        double Value { get; }
    }
}