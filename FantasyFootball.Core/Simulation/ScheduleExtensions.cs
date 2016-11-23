﻿using System.Linq;

namespace FantasyFootball.Core.Simulation
{
    public static class ScheduleExtensions
    {
        public static Core.Matchup[] GetMatchups(this Universe universe, int week)
        {
            return universe.Facts.OfType<AddMatchup>()
                .Select(f => f.Matchup)
                .Where(m => m.Week == week)
                .ToArray();
        }
    }
}