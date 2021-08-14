using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Objects
{
    public static class Players
    {
        private const string filename = "data.csv";
        private static IEnumerable<Player> all;
        private static DateTime lastModified = DateTime.MinValue;
        public static IEnumerable<Player> All()
        {
            if (new FileInfo(filename).LastWriteTime > lastModified)
            {
                var lines = File.ReadAllLines(filename).Select(l => l.Split(','));

                all = lines.Select(l => new Player
                {
                    Id = l[0],
                    Name = l[1],
                    Team = l[2],
                    Positions = l[3].Split('/')
                });
                lastModified = new FileInfo(filename).LastWriteTime;
            }
            return all;
        }

        public static Player From(FantasyFootball.Data.Yahoo.Models.Player player)
        {
            return new Player
            {
                Id = player.player_id.ToString(),
                Name = player.name.full,
                Positions = player.display_position.Split(','),
                Team = player.editorial_team_abbr
            };
        }
    }
}
