using System.Linq;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Service.Football.Models;

namespace FantasyFootball.Service.Football.Actions
{
    public class RebuildFootballDatabase
    {
        public void Rebuild()
        {
            var yahoo = new FantasySportsService();
            using (var footballContext = new FootballContext())
            {
                var players = yahoo.Players("nfl").Select(p => new Player
                {
                    Id = p.player_id,
                    Name = p.name.full,
                    Image = p.image_url
                });
                footballContext.AddRange(players);

                footballContext.SaveChanges(true);
            }
        }
    }
}
