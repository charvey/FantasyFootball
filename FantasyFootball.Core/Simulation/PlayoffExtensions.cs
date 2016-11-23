using FantasyFootball.Core.Draft;

namespace FantasyFootball.Core.Simulation
{
    public static class PlayoffExtensions
    {
        public static Team GetChampionshipWinner(this Universe universe)
        {
            return universe.GetWinner(new Core.Matchup
            {
                TeamA = universe.GetSemifinalAWinner(),
                TeamB = universe.GetSemifinalBWinner(),
                Week = 16
            });
        }

        public static Team GetSemifinalAWinner(this Universe universe)
        {
            return universe.GetWinner(new Core.Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(1),
                TeamB = universe.GetQuarterFinalAWinner(),
                Week = 15
            });
        }

        public static Team GetSemifinalBWinner(this Universe universe)
        {
            return universe.GetWinner(new Core.Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(2),
                TeamB = universe.GetQuarterFinalBWinner(),
                Week = 15
            });
        }

        public static Team GetQuarterFinalAWinner(this Universe universe)
        {
            return universe.GetWinner(new Core.Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(4),
                TeamB = universe.GetTeamInPlaceAtEndOfSeason(5),
                Week = 14
            });
        }

        public static Team GetQuarterFinalBWinner(this Universe universe)
        {
            return universe.GetWinner(new Core.Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(3),
                TeamB = universe.GetTeamInPlaceAtEndOfSeason(6),
                Week = 14
            });
        }
    }
}
