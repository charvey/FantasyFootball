using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace FantasyFootball.Terminal.Database
{
    public static class PredictionOperations
    {
        public static void AddPrediction(this SQLiteConnection connection, string playerId, int week, int year, double value, DateTime asOf)
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

        public static double GetPrediction(this SQLiteConnection connection, string playerId, int year, int week)
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

        public static double[] GetPredictions(this SQLiteConnection connection, string playerId, int year, IEnumerable<int> weeks)
        {
            return weeks.Select(w => connection.GetPrediction(playerId, year, w)).ToArray();
        }
    }
}
