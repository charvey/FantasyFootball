using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyFootball.Core.Players
{
    public class Player
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Position { get; set; }
        public string Team { get; set; }

        public override bool Equals(object obj)
        {
            var otherPlayer = obj as Player;
            if (otherPlayer == null) return false;
            return otherPlayer.Id == this.Id;
        }

        public override int GetHashCode()
        {
            return this.Id.GetHashCode();
        }

        private const string filename = "data.csv";
        private static IEnumerable<Player> all;
        private static DateTime lastModified = DateTime.MinValue;
        public static IEnumerable<Player> All()
        {
            if (new FileInfo(filename).LastWriteTime > lastModified)
            {
                var lines = File.ReadAllLines(filename).Select(l => l.Split(','));

                //Temporary filter
                //lines = lines.Where(l => !l[3].Contains('/'));

                all = lines.Select(l => new Player
                {
                    Id = l[0],
                    Name = l[1],
                    Team = l[2],
                    Position = l[3]
                });
                lastModified = new FileInfo(filename).LastWriteTime;
            }
            return all;
        }

        public static Player Get(string id)
        {
            return All().Single(p => p.Id == id);
        }

        public static Player From(Data.Yahoo.Models.Player player)
        {
            if (player.display_position == "DEF")
                return Player.All().Single(x => x.Position == "DEF" && x.Team == player.editorial_team_abbr);
            else
                return Player.Get(player.player_id);
        }
    }
}
