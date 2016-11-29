using System;

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

        public static int ComputeCurrentWeek(DateTime start, DateTime now)
        {
            var timeSinceSeasonStart = now - start;
            var weeksSinceSeasonStart = Math.Ceiling(timeSinceSeasonStart.TotalDays / 7);
            return (int)Math.Max(1, Math.Min(17, weeksSinceSeasonStart));
        }
    }
}
