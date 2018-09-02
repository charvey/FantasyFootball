using System;
using System.Collections.Generic;
using Yahoo;

namespace FantasyFootball.Core.Data
{
    public interface IPredictionRepository
    {
        void AddPrediction(LeagueKey leagueKey, string playerId, int week, double value, DateTime asOf);
        double GetPrediction(LeagueKey leagueKey, string playerId, int week);
        double[] GetPredictions(LeagueKey leagueKey, string playerId, IEnumerable<int> weeks);
    }
}