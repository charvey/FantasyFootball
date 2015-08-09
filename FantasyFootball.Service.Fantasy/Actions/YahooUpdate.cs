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
                foreach(var league in yahoo.Leagues())
                {
                    fantasyContext.Leagues.Add(new League
                    {
                        Id = league.league_key,
                        Name = league.name
                    });
                }
                fantasyContext.SaveChanges(true);
            }
        }
    }
}
