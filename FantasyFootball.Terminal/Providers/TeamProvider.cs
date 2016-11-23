using FantasyFootball.Terminal.GameStateModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Terminal.Providers
{
    public class TeamProvider
    {
        private string filename;

        public TeamProvider(string filename)
        {
            this.filename = filename;
        }

        public IEnumerable<Team> Provide()
        {
            return File.ReadAllLines(filename).Select(Parse);
        }

        private Team Parse(string line)
        {
            var values = line.Split(',');

            return new Team
            {
                Id = values[0],
                Name = values[1],
                Owner = values[2],
                DraftOrder = int.Parse(values[3])
            };
        }
    }
}
