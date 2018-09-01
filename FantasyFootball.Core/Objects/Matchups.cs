using FantasyFootball.Data.Yahoo;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FantasyFootball.Core.Objects
{
    public static class Matchups
    {
        public static IEnumerable<Matchup> GetByWeek(FantasySportsService service, string league_key, int week)
        {
            var scoreboard = service.LeagueScoreboard(league_key, week);
            var teams = service.Teams(league_key).Select(Teams.From);
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
