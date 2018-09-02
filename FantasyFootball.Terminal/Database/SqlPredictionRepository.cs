using Dapper;
using FantasyFootball.Core.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using Yahoo;

namespace FantasyFootball.Terminal.Database
{
    public class SqlPredictionRepository : IPredictionRepository
    {
        private readonly SQLiteConnection connection;

        public SqlPredictionRepository(SQLiteConnection connection)
        {
            this.connection = connection;
        }

        public void AddPrediction(LeagueKey leagueKey, string playerId, int week, double value, DateTime asOf)
        {
            connection.Execute(@"
                INSERT INTO Predictions (LeagueKey,PlayerId,Week,Value,AsOf)
                VALUES (@LeagueKey,@PlayerId,@Week,@Value,@AsOf)", new
            {
                LeagueKey = leagueKey.ToString(),
                PlayerId = playerId,
                Week = week,
                Value = value,
                AsOf = asOf.ToString("O")
            });
        }

        public double GetPrediction(LeagueKey leagueKey, string playerId, int week)
        {
            return connection.QueryFirst<double>(@"
                SELECT Value
                FROM Predictions
                WHERE LeagueKey=@leagueKey AND PlayerId=@playerId AND Week=@week
                ORDER BY AsOf DESC", new
            {
                leagueKey = leagueKey.ToString(),
                playerId = playerId,
                week = week
            });
        }

        public double[] GetPredictions(LeagueKey leagueKey, string playerId, IEnumerable<int> weeks)
        {
            return weeks.Select(w => GetPrediction(leagueKey, playerId, w)).ToArray();
        }
    }

    public class CachedPredictionRepository : IPredictionRepository
    {
        private readonly IPredictionRepository predictionRepository;

        public CachedPredictionRepository(IPredictionRepository predictionRepository)
        {
            this.predictionRepository = predictionRepository;
        }

        public void AddPrediction(LeagueKey leagueKey, string playerId, int week, double value, DateTime asOf)
        {
            throw new NotImplementedException();
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
