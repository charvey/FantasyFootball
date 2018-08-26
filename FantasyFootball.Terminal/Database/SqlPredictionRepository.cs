using Dapper;
using FantasyFootball.Core.Data;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace FantasyFootball.Terminal.Database
{
    public class SqlPredictionRepository : IPredictionRepository
    {
        private readonly SQLiteConnection connection;

        public SqlPredictionRepository(SQLiteConnection connection)
        {
            this.connection = connection;
        }

        public void AddPrediction(string playerId, int week, int year, double value, DateTime asOf)
        {
            connection.Execute(@"
                INSERT INTO Predictions (PlayerId,Week,Year,Value,AsOf)
                VALUES (@PlayerId,@Week,@Year,@Value,@AsOf)", new
            {
                PlayerId = playerId,
                Week = week,
                Year = year,
                Value = value,
                AsOf = asOf.ToString("O")
            });
        }

        //TODO replace year with league identifier
        public double GetPrediction(string playerId, int year, int week)
        {
            return connection.QueryFirst<double>(@"
                SELECT Value
                FROM Predictions
                WHERE PlayerId=@playerId AND Year=@year AND Week=@week
                ORDER BY AsOf DESC", new
            {
                playerId = playerId,
                year = year,
                week = week
            });
        }

        public double[] GetPredictions(string playerId, int year, IEnumerable<int> weeks)
        {
            return weeks.Select(w => GetPrediction(playerId, year, w)).ToArray();
        }
    }

    public class CachedPredictionRepository : IPredictionRepository
    {
        private readonly IPredictionRepository predictionRepository;

        public CachedPredictionRepository(IPredictionRepository predictionRepository)
        {
            this.predictionRepository = predictionRepository;
        }

        public void AddPrediction(string playerId, int week, int year, double value, DateTime asOf)
        {
            throw new NotImplementedException();
        }

        private readonly ConcurrentDictionary<Tuple<string, int, int>, double> cache = new ConcurrentDictionary<Tuple<string, int, int>, double>();
        public double GetPrediction(string playerId, int year, int week)
        {
            return cache.GetOrAdd(Tuple.Create(playerId, year, week), x => predictionRepository.GetPrediction(x.Item1, x.Item2, x.Item3));
        }

        public double[] GetPredictions(string playerId, int year, IEnumerable<int> weeks)
        {
            return weeks.Select(w => GetPrediction(playerId, year, w)).ToArray();
        }
    }
}
