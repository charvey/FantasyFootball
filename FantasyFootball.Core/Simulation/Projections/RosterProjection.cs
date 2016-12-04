using FantasyFootball.Core.Objects;
using System;
using System.Collections.Generic;

namespace FantasyFootball.Core.Simulation.Projections
{
    public class RosterProjection : Projection<Dictionary<Tuple<int, int>, Player[]>>
    {
        public static Player[] GetRoster(Universe universe, Team team, int week)
        {
            return GetState(universe)[Tuple.Create(team.Id, week)];
        }

        public static void SetRoster(Universe universe, Team team, int week, Player[] players)
        {
            GetState(universe)[Tuple.Create(team.Id, week)] = players;
        }

        protected override Dictionary<Tuple<int, int>, Player[]> Clone(Dictionary<Tuple<int, int>, Player[]> original)
        {
            return new Dictionary<Tuple<int, int>, Player[]>(original);
        }
    }
}
