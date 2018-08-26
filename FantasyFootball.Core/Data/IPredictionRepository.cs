using System;
using System.Collections.Generic;

namespace FantasyFootball.Core.Data
{
    public interface IPredictionRepository
    {
        void AddPrediction(string playerId, int week, int year, double value, DateTime asOf);
        double GetPrediction(string playerId, int year, int week);
        double[] GetPredictions(string playerId, int year, IEnumerable<int> weeks);
    }
}