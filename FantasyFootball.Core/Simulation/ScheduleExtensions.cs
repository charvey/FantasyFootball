using FantasyFootball.Core.Objects;
using System.Linq;

namespace FantasyFootball.Core.Simulation
{
    public static class ScheduleExtensions
    {
        public static Matchup[] GetMatchups(this Universe universe, int week)
        {
            return universe.Facts.OfType<AddMatchup>()
                .Select(f => f.Matchup)
                .Where(m => m.Week == week)
                .ToArray();
        }
    }
}
