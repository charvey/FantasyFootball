using FantasyFootball.Core.Objects;
using FantasyFootball.Core.Simulation.Projections;
using System;
using System.Linq;

namespace FantasyFootball.Core.Simulation
{
    public static class GeneralExtensions
    {
        public static MatchupResult GetRegularSeasonResult(this Universe universe, Matchup matchup)
        {
            var teamAScore = universe.GetScore(matchup.TeamA, matchup.Week);
            var teamBScore = universe.GetScore(matchup.TeamB, matchup.Week);

            if (teamAScore > teamBScore) return new MatchupResult { Winner = matchup.TeamA, Loser = matchup.TeamB };
            else if (teamAScore < teamBScore) return new MatchupResult { Winner = matchup.TeamB, Loser = matchup.TeamA };
            else return new MatchupResult { Tied = true };
        }

        public static MatchupResult GetPlayoffResult(this Universe universe, Matchup matchup)
        {
            {
                var teamAScore = universe.GetScore(matchup.TeamA, matchup.Week);
                var teamBScore = universe.GetScore(matchup.TeamB, matchup.Week);

                if (teamAScore > teamBScore) return new MatchupResult { Winner = matchup.TeamA, Loser = matchup.TeamB };
                else if (teamAScore < teamBScore) return new MatchupResult { Winner = matchup.TeamB, Loser = matchup.TeamA };
            }
            {
                var regularSeasonMatchups = SeasonWeek.RegularSeasonWeeks
                    .SelectMany(w => MatchupProjection.GetMatchups(universe, w))
                    .Where(m => (m.TeamA == matchup.TeamA && m.TeamB == matchup.TeamB) || (m.TeamA == matchup.TeamB && m.TeamB == matchup.TeamA));
                var regularSeasonWinners = regularSeasonMatchups.Select(m => universe.GetRegularSeasonResult(m).Winner);
                var teamAWins = regularSeasonWinners.Count(w => w == matchup.TeamA);
                var teamBWins = regularSeasonWinners.Count(w => w == matchup.TeamB);
                if (teamAWins > teamBWins) return new MatchupResult { Winner = matchup.TeamA, Loser = matchup.TeamB };
                else if (teamAWins < teamBWins) return new MatchupResult { Winner = matchup.TeamB, Loser = matchup.TeamA };
            }
            {
                var teamASeed = universe.GetSeedAtEndOfSeason(matchup.TeamA);
                var teamBSeed = universe.GetSeedAtEndOfSeason(matchup.TeamB);
                if (teamASeed < teamBSeed) return new MatchupResult { Winner = matchup.TeamA, Loser = matchup.TeamB };
                else if (teamASeed > teamBSeed) return new MatchupResult { Winner = matchup.TeamB, Loser = matchup.TeamA };
            }
            throw new InvalidOperationException();
        }

        public static double GetScore(this Universe universe, Team team, int week)
        {
            return universe.GetRoster(team, week).Sum(p => universe.GetScore(p, week));
        }

        public static double GetScore(this Universe universe, Player player, int week)
        {
            return ScoreProjection.GetScore(universe, player, week);
        }

        public static Player[] GetRoster(this Universe universe, Team team, int week)
        {
            return RosterProjection.GetRoster(universe, team, week);
        }

        public static Team[] GetTeams(this Universe universe)
        {
            return TeamProjection.GetTeams(universe);
        }
    }
}
