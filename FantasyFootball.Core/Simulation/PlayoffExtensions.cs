using FantasyFootball.Core.Objects;

namespace FantasyFootball.Core.Simulation
{
    public static class PlayoffExtensions
    {
        public static MatchupResult GetChampionshipResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetSemifinalAResult().Winner,
                TeamB = universe.GetSemifinalBResult().Winner,
                Week = SeasonWeek.ChampionshipWeek
            });
        }

        public static MatchupResult Get3rdPlaceGameResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetSemifinalAResult().Loser,
                TeamB = universe.GetSemifinalBResult().Loser,
                Week = SeasonWeek.ChampionshipWeek
            });
        }

        public static MatchupResult Get5thPlaceGameResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetQuarterfinalAResult().Loser,
                TeamB = universe.GetQuarterfinalBResult().Loser,
                Week = SeasonWeek.SemifinalWeek
            });
        }

        public static MatchupResult GetSemifinalAResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(1),
                TeamB = universe.GetQuarterfinalAResult().Winner,
                Week = SeasonWeek.SemifinalWeek
            });
        }

        public static MatchupResult GetSemifinalBResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(2),
                TeamB = universe.GetQuarterfinalBResult().Winner,
                Week = SeasonWeek.SemifinalWeek
            });
        }

        public static MatchupResult GetQuarterfinalAResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(4),
                TeamB = universe.GetTeamInPlaceAtEndOfSeason(5),
                Week = SeasonWeek.QuarterFinalWeek
            });
        }

        public static MatchupResult GetQuarterfinalBResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(3),
                TeamB = universe.GetTeamInPlaceAtEndOfSeason(6),
                Week = SeasonWeek.QuarterFinalWeek
            });
        }

        public static MatchupResult Get7thPlaceGameResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetConsolationSemifinalAResult().Winner,
                TeamB = universe.GetConsolationSemifinalBResult().Winner,
                Week = SeasonWeek.ChampionshipWeek
            });
        }

        public static MatchupResult Get9thPlaceGameResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetConsolationSemifinalAResult().Loser,
                TeamB = universe.GetConsolationSemifinalBResult().Loser,
                Week = SeasonWeek.ChampionshipWeek
            });
        }

        public static MatchupResult Get11thPlaceGameResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetConsolationQuarterfinalAResult().Loser,
                TeamB = universe.GetConsolationQuarterfinalBResult().Loser,
                Week = SeasonWeek.ChampionshipWeek
            });
        }

        public static MatchupResult GetConsolationSemifinalAResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(7),
                TeamB = universe.GetConsolationQuarterfinalAResult().Winner,
                Week = SeasonWeek.SemifinalWeek
            });
        }

        public static MatchupResult GetConsolationSemifinalBResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(8),
                TeamB = universe.GetConsolationQuarterfinalBResult().Winner,
                Week = SeasonWeek.SemifinalWeek
            });
        }

        public static MatchupResult GetConsolationQuarterfinalAResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(10),
                TeamB = universe.GetTeamInPlaceAtEndOfSeason(11),
                Week = SeasonWeek.QuarterFinalWeek
            });
        }

        public static MatchupResult GetConsolationQuarterfinalBResult(this Universe universe)
        {
            return universe.GetPlayoffResult(new Matchup
            {
                TeamA = universe.GetTeamInPlaceAtEndOfSeason(9),
                TeamB = universe.GetTeamInPlaceAtEndOfSeason(12),
                Week = SeasonWeek.QuarterFinalWeek
            });
        }
    }
}
