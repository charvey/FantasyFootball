﻿using FantasyFootball.Data.Yahoo;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FantasyFootball.Core.Objects
{
    public static class Matchups
    {
        public static IEnumerable<Matchup> GetByWeek(int week)
        {
            var service = new FantasySportsService();
            var scoreboard = service.LeagueScoreboard("359.l.48793", week);
            return scoreboard.matchups.Select(m =>
            {
                Debug.Assert(m.teams.Length == 2);
                return new Matchup
                {
                    TeamA = Teams.Get(int.Parse(m.teams.First().team_id)),
                    TeamB = Teams.Get(int.Parse(m.teams.Last().team_id)),
                    Week = m.week
                };
            });
        }
    }
}
