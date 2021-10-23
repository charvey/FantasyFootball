namespace FantasyFootball.Core.Simulation
{
    public static class SeasonWeekExtensions
    {
        public static int GetChampionshipWeek(this Universe universe) => SeasonWeek.ChampionshipWeek;
        public static int GetSemifinalWeek(this Universe universe) => SeasonWeek.SemifinalWeek;
        public static int GetQuarterFinalWeek(this Universe universe) => SeasonWeek.QuarterFinalWeek;
        public static int GetRegularSeasonEnd(this Universe universe) => SeasonWeek.RegularSeasonEnd;
        public static IEnumerable<int> GetRegularSeasonWeeks(this Universe universe) => SeasonWeek.RegularSeasonWeeks;
    }
}
