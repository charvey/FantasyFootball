using FantasyFootball.Terminal.GameStateModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Terminal.Providers
{
    public class PlayerProvider
    {
        private string filename;

        public PlayerProvider(string filename)
        {
            this.filename = filename;
        }

        public IEnumerable<Player> Provide()
        {
            return File.ReadAllLines(filename)
                .Select(Parse);
        }

        private Player Parse(string line)
        {
            var values = line.Split(',');

            return new Player
            {
                Id = values[0],
                Name = values[1],
                Team = values[2],
                Positions = new HashSet<string>(values[3].Split('/'))
            };
        }
    }
}
