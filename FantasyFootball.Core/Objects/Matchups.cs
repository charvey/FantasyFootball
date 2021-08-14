using FantasyFootball.Data.Yahoo;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Yahoo;

namespace FantasyFootball.Core.Objects
{
    public static class Matchups
    {
        public static IEnumerable<Matchup> GetByWeek(FantasySportsService service, LeagueKey leagueKey, int week)
        {
            var scoreboard = service.LeagueScoreboard(leagueKey, week);
            var teams = service.Teams(leagueKey).Select(Teams.From);
            return scoreboard.matchups.Select(m =>
            {
                Debug.Assert(m.teams.Length == 2);
                return new Matchup
                {
                    TeamA = teams.Single(t => t.Id == m.teams.First().team_id),
                    TeamB = teams.Single(t => t.Id == m.teams.Last().team_id),
                    Week = m.week
                };
            });
        }
    }
}
