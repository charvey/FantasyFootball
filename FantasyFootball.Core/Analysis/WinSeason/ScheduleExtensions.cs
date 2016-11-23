using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyFootball.Core.Analysis.WinSeason
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
