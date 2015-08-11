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
                    var players = yahoo.Players(l.league_key).Select(p => new Player
                    {
                        Id = p.player_key,
                        Name = p.name.full
                    });
                    league.Players = new HashSet<Player>(players);

                    fantasyContext.Leagues.Add(league);
                    fantasyContext.Players.AddRange(players);
                    fantasyContext.Teams.AddRange(teams);
                }
                fantasyContext.SaveChanges(true);
            }
        }
    }
}
