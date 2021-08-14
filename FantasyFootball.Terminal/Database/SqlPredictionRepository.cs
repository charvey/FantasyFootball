using Dapper;
using FantasyFootball.Core.Data;
using System.Collections.Concurrent;
using System.Data;
using System.Data.SQLite;
using Yahoo;

namespace FantasyFootball.Terminal.Database
{
    internal class LeagueKeyTypeHandler : SqlMapper.TypeHandler<LeagueKey>
    {
        public override LeagueKey Parse(object value)
        {
            return LeagueKey.Parse(value.ToString());
        }

        public override void SetValue(IDbDataParameter parameter, LeagueKey value)
        {
            parameter.Value = value.ToString();
        }
    }

    public class SqlPredictionRepository : ILatestPredictionRepository, IFullPredictionRepository
    {
        private class PredictionRow : Prediction
        {
            public LeagueKey LeagueKey { get; set; }
            public string PlayerId { get; set; }
            public int Week { get; set; }
            public string AsOf { get; set; }
            DateTime Prediction.AsOf => DateTime.Parse(AsOf);
            public double Value { get; set; }
        }

        private readonly SQLiteConnection connection;

        public SqlPredictionRepository(SQLiteConnection connection)
        {
            this.connection = connection;
            SqlMapper.AddTypeHandler(new LeagueKeyTypeHandler());
        }

        public void AddPrediction(LeagueKey leagueKey, string playerId, int week, double value, DateTime asOf)
        {
            connection.Execute(@"
                INSERT INTO Predictions (LeagueKey,PlayerId,Week,Value,AsOf)
                VALUES (@LeagueKey,@PlayerId,@Week,@Value,@AsOf)", new
            {
                LeagueKey = leagueKey,
                PlayerId = playerId,
                Week = week,
                Value = value,
                AsOf = asOf.ToString("O")
            });
        }

        public IReadOnlyList<Prediction> GetAll(LeagueKey leagueKey)
        {
            return connection.Query<PredictionRow>(@"
                SELECT * FROM Predictions WHERE LeagueKey=@LeagueKey", new
            {
                LeagueKey = leagueKey
            }).ToList();
        }

        public double GetPrediction(LeagueKey leagueKey, string playerId, int week)
        {
            return connection.QueryFirst<double>(@"
                SELECT Value
                FROM Predictions
                WHERE LeagueKey=@leagueKey AND PlayerId=@playerId AND Week=@week
                ORDER BY date(AsOf) DESC", new
            {
                leagueKey = leagueKey,
                playerId = playerId,
                week = week
            });
        }

        public double[] GetPredictions(LeagueKey leagueKey, string playerId, IEnumerable<int> weeks)
        {
            return weeks.Select(w => GetPrediction(leagueKey, playerId, w)).ToArray();
        }
    }

    public class CachedPredictionRepository : ILatestPredictionRepository
    {
        private readonly ILatestPredictionRepository predictionRepository;

        public CachedPredictionRepository(ILatestPredictionRepository predictionRepository)
        {
            this.predictionRepository = predictionRepository;
        }

        private readonly ConcurrentDictionary<Tuple<LeagueKey, string, int>, double> cache = new ConcurrentDictionary<Tuple<LeagueKey, string, int>, double>();
        public double GetPrediction(LeagueKey leagueKey, string playerId, int week)
        {
            return cache.GetOrAdd(Tuple.Create(leagueKey, playerId, week), x => predictionRepository.GetPrediction(x.Item1, x.Item2, x.Item3));
        }

        public double[] GetPredictions(LeagueKey leagueKey, string playerId, IEnumerable<int> weeks)
        {
            return weeks.Select(w => GetPrediction(leagueKey, playerId, w)).ToArray();
        }
    }
}
