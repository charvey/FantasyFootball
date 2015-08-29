using FantasyFootball.Data.Yahoo;
using FantasyFootball.Service.Fantasy.Models;
using System;
using System.Collections.Generic;
using System.Linq;

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
                        Count = rp.count,
                        League = league
                    });
                    league.RosterPositions = new HashSet<RosterPosition>(positions);
                    fantasyContext.RosterPositions.AddRange(positions);
                    fantasyContext.SaveChanges(true);

                    var players = yahoo.Players(l.league_key).Take(15).Select(p => {
                        var player = new LeaguePlayer
                        {
                            Id = p.player_key,
                            PlayerId = p.player_id,
                            ByeWeek = p.bye_weeks.Single().value,
                            Positions = new HashSet<PlayerPosition>(),
                            Teams = new HashSet<Team>(),
                            League = league
                        };

                        foreach(var ep in p.eligible_positions)
                        {
                            var pp = new PlayerPosition
                            {
                                Id = Guid.NewGuid(),
                                Player = player,
                                Position = positions.Single(pos => pos.Position == ep.value)
                            };
                            //player.Positions.Add(pp);
                            fantasyContext.PlayerPosition.Add(pp);
                        }

                        return player;
                    });
                    league.Players = new HashSet<LeaguePlayer>(players);
                    fantasyContext.LeaguePlayers.AddRange(players);
                    fantasyContext.SaveChanges(true);
                }
                fantasyContext.SaveChanges(true);
            }
        }
    }
}
