using FantasyFootball.Data.Yahoo;
using FantasyFootball.Service.Fantasy.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyFootball.Service.Fantasy.Actions
{
    public class RebuildFantasyDatabase
    {
        public void Rebuild()
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
                    fantasyContext.Leagues.Add(league);
                    fantasyContext.SaveChanges(true);

                    var teams = yahoo.Teams(l.league_key).Select(t => new Team
                    {
                        Id = t.team_key,
                        Name = t.name,
                        League = league
                    });
                    league.Teams = new HashSet<Team>(teams);
                    fantasyContext.Teams.AddRange(teams);
                    fantasyContext.SaveChanges(true);

                    var positions = l.settings.roster_positions.Select(rp => new RosterPosition
                    {
                        Id = league.Id + ":" + rp.position,
                        Position = rp.position,
                        EligiblePositions = new[] { rp.position_type },
                        Count = rp.count,
                        League = league
                    });
                    league.RosterPositions = new HashSet<RosterPosition>(positions);
                    fantasyContext.RosterPositions.AddRange(positions);
                    fantasyContext.SaveChanges(true);

                    var players = yahoo.Players(l.league_key).Take(15).Select(p => new Player
                    {
                        Id = p.player_key,
                        PlayerId = p.player_id,
                        ByeWeek = p.bye_weeks.Single().value,
                        League = league
                    });
                    league.Players = new HashSet<Player>(players);
                    fantasyContext.Players.AddRange(players);
                    fantasyContext.SaveChanges(true);
                }
                fantasyContext.SaveChanges(true);
            }
        }
    }
}
