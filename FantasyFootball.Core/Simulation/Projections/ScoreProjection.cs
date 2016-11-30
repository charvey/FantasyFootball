using FantasyFootball.Core.Objects;
using FantasyFootball.Core.Simulation.Facts;
using System;
using System.Collections.Generic;

namespace FantasyFootball.Core.Simulation.Projections
{
    public class ScoreProjection : Projection<Dictionary<Tuple<string, int>, double>>
    {
        public static double GetScore(Universe universe, Player player, int week)
        {
            return GetState(universe)[Tuple.Create(player.Id, week)];
        }

        public static void SetScore(Universe universe, Player player, int week, double score)
        {
            GetState(universe)[Tuple.Create(player.Id, week)] = score;
        }
    }
}
