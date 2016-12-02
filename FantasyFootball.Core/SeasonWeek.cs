using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Core
{
    public static class SeasonWeek
    {
        public static int Current
        {
            get
            {
                if (DateTime.Now > new DateTime(2017, 2, 5)) throw new ArgumentOutOfRangeException();
                return ComputeCurrentWeek(new DateTime(2016, 9, 6), DateTime.Now);
            }
        }

        public static int RegularSeasonEnd => 13;
        public static int QuarterFinalWeek => 14;
        public static int SemifinalWeek => 15;
        public static int ChampionshipWeek => 16;
        public static int Maximum => 17;

        public static IEnumerable<int> RegularSeasonWeeks => Enumerable.Range(1, RegularSeasonEnd);

        public static int ComputeCurrentWeek(DateTime start, DateTime now)
        {
            var timeSinceSeasonStart = now - start;
            var weeksSinceSeasonStart = Math.Ceiling(timeSinceSeasonStart.TotalDays / 7);
            return (int)Math.Max(1, Math.Min(Maximum, weeksSinceSeasonStart));
        }
    }
}
