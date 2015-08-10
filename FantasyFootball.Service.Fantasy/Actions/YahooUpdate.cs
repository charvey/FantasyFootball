using FantasyFootball.Data.Yahoo;
using FantasyFootball.Service.Fantasy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyFootball.Service.Fantasy.Actions
{
    public class FantasyUpdater
    {
        public void Update()
        {
            var yahoo = new FantasySportsService();
            using (var fantasyContext = new FantasyContext())
            {
                foreach(var l in yahoo.Leagues())
                {
                    var league = new League
                    {
                        Id = l.league_key,
                        Name = l.name
                    };
                    var teams = yahoo.Teams(l.league_key).Select(t => new Team
                    {
                        Id = t.team_key,
                        Name = t.name,
                        League = league
                    });
                    league.Teams = new HashSet<Team>(teams);

                    fantasyContext.Leagues.Add(league);
                    fantasyContext.Teams.AddRange(teams);
                }
                fantasyContext.SaveChanges(true);
            }
        }
    }
}
