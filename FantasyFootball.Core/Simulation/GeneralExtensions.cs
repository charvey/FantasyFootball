using FantasyFootball.Core.Objects;
using FantasyFootball.Core.Simulation.Projections;
using System;
using System.Linq;

namespace FantasyFootball.Core.Simulation
{
    public static class GeneralExtensions
    {
        public static Team GetWinner(this Universe universe, Matchup matchup)
        {
            var teamAScore = universe.GetScore(matchup.TeamA, matchup.Week);
            var teamBScore = universe.GetScore(matchup.TeamB, matchup.Week);

            if (teamAScore > teamBScore) return matchup.TeamA;
            else if (teamAScore < teamBScore) return matchup.TeamB;
            throw new NotImplementedException();
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
