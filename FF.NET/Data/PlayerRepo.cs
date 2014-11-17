using Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data
{
    public static class PlayerRepo
    {
        private static List<Player> _players;
        public static IEnumerable<Player> GetPlayers()
        {
            if (_players == null)
            {
                var dataset = DataSet.fromCSV(Path.Combine(Config.DIR, "players.csv"));
                _players = dataset.Rows.Select(d => new Player
                {
                    Id = d["Id"],
                    Name = d["Name"],
                    Position = (Position)Enum.Parse(typeof(Position), d["Position"])
                }).ToList();
            }

            return _players;
        }

        public static Player GetPlayer(string id)
        {
            return GetPlayers().Single(p => p.Id == id);
        }
    }
}
