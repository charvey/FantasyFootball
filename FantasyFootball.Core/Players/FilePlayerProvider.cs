using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyFootball.Core.Players
{
    public class FilePlayerProvider : PlayerProvider
    {
        public IEnumerable<Player> GetPlayers()
        {
            var lines = File.ReadAllLines("data.csv").Select(l => l.Split(','));

            //Temporary filter
            lines = lines.Where(l => !l[3].Contains('/'));

            return lines.Select(l => new Player
            {
                Id = l[0],
                Name = l[1],
                Position = l[3]
            });
        }
    }
}
