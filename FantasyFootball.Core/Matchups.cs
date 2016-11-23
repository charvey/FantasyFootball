using FantasyFootball.Core.Draft;
using FantasyFootball.Data.Yahoo;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FantasyFootball.Core
{
    public class Matchup
    {
        public Team TeamA { get; set; }
        public Team TeamB { get; set; }
        public int Week { get; set; }

        public static IEnumerable<Matchup> Matchups(int week)
        {
            var service = new FantasySportsService();
            var scoreboard = service.LeagueScoreboard("359.l.48793", week);
            return scoreboard.matchups.Select(m =>
            {
                Debug.Assert(m.teams.Length == 2);
                return new Matchup
                {
                    TeamA = Team.Get(int.Parse(m.teams.First().team_id)),
                    TeamB = Team.Get(int.Parse(m.teams.Last().team_id)),
                    Week = m.week
                };
            });
        }
    }
}
