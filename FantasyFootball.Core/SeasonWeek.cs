﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Core
{
    [Obsolete]
    public static class SeasonWeek
    {
        public static int RegularSeasonEnd => 13;
        public static int QuarterFinalWeek => 14;
        public static int SemifinalWeek => 15;
        public static int ChampionshipWeek => 17;
        public static int Maximum => 17;

        public static IEnumerable<int> RegularSeasonWeeks => Enumerable.Range(1, RegularSeasonEnd);
    }
}
