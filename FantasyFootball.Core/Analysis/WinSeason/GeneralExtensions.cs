using FantasyFootball.Core.Draft;
using FantasyFootball.Core.Players;
using System;
using System.Linq;

namespace FantasyFootball.Core.Analysis.WinSeason
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

        public static double GetScore(this Universe universe, Player player, int week)
        {
            return universe.Facts.OfType<SetScore>()
                .Single(f => f.Player.Id == player.Id && f.Week == week).Score;
        }

        public static Player[] GetRoster(this Universe universe, Team team, int week)
        {
            return universe.Facts.OfType<SetRoster>()
                .Single(f => f.Team.Id == team.Id && f.Week == week).Players;
        }

        public static Team[] GetTeams(this Universe universe)
        {
            return universe.Facts.OfType<AddTeam>().Select(f => f.Team).ToArray();
        }
    }
}
