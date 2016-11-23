using FantasyFootball.Core.Draft;
using FantasyFootball.Core.Players;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace FantasyFootball.Core.Simulation
{
    public static class GeneralExtensions
    {
        public static Team GetWinner(this Universe universe, Core.Matchup matchup)
        {
            var teamAScore = universe.GetScore(matchup.TeamA, matchup.Week);
            var teamBScore = universe.GetScore(matchup.TeamB, matchup.Week);

            if (teamAScore > teamBScore) return matchup.TeamA;
            else if (teamAScore < teamBScore) return matchup.TeamB;
            throw new NotImplementedException();
        }

        public static double GetScore(this Universe universe, Team team, int week)
        {
            return universe.GetRoster(team, week)
                .Sum(p => universe.GetScore(p, week));
        }

        private static ConcurrentDictionary<Guid, ScoreProjection> scoreProjections = new ConcurrentDictionary<Guid, ScoreProjection>();
        public static double GetScore(this Universe universe, Player player, int week)
        {
            return scoreProjections.GetOrAdd(universe.Id, _ => new ScoreProjection(universe)).GetScore(player, week);
        }

        private static ConcurrentDictionary<Guid, RosterProjection> rosterProjections = new ConcurrentDictionary<Guid, RosterProjection>();
        public static Player[] GetRoster(this Universe universe, Team team, int week)
        {
            return rosterProjections.GetOrAdd(universe.Id, _ => new RosterProjection(universe)).GetRoster(team, week);
        }

        private static ConcurrentDictionary<Guid, TeamProjection> teamProjections = new ConcurrentDictionary<Guid, TeamProjection>();
        public static Team[] GetTeams(this Universe universe)
        {
            return teamProjections.GetOrAdd(universe.Id, _ => new TeamProjection(universe)).GetTeams();
        }
    }
}
