using FantasyFootball.Core.Objects;

namespace FantasyFootball.Core.Simulation
{
    public static class PlayoffExtensions
    {
        public static Team GetChampionshipWinner(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetSemifinalAWinner(),
                TeamB = universe.GetSemifinalBWinner(),
                Week = SeasonWeek.ChampionshipWeek
            }).Winner;
        }

        public static Team GetSemifinalAWinner(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(1),
                TeamB = universe.GetQuarterFinalAWinner(),
                Week = SeasonWeek.SemifinalWeek
            }).Winner;
        }

        public static Team GetSemifinalBWinner(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(2),
                TeamB = universe.GetQuarterFinalBWinner(),
                Week = SeasonWeek.SemifinalWeek
            }).Winner;
        }

        public static Team GetQuarterFinalAWinner(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(4),
                TeamB = universe.GetTeamInPlaceAtEndOfSeason(5),
                Week = SeasonWeek.QuarterFinalWeek
            }).Winner;
        }

        public static Team GetQuarterFinalBWinner(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(3),
                TeamB = universe.GetTeamInPlaceAtEndOfSeason(6),
                Week = SeasonWeek.QuarterFinalWeek
            }).Winner;
        }
    }
}
