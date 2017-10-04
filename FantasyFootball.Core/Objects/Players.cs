using System;
using System.Collections.Generic;
using System.Diagnostics;
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
            Debug.Assert(player.display_position.All(char.IsLetter));
            return new Player
            {
                Id = player.player_id,
                Name = player.name.full,
                Positions = new[] { player.display_position },
                Team = player.editorial_team_abbr
            };
        }
    }
}
