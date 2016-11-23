using FantasyFootball.Terminal.GameStateModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Terminal.Providers
{
    public class MatchupProvider
    {
        private string filename;

        public MatchupProvider(string filename)
        {
            this.filename = filename;
        }

        public IEnumerable<Matchup> Provide(IEnumerable<Team> teams)
        {
            return File.ReadAllLines(filename)
               .SelectMany((l, w) => ParseWeek(w + 1, l, teams));
        }

        private IEnumerable<Matchup> ParseWeek(int week, string line, IEnumerable<Team> teams)
        {
            return line.Split(',').Select(m => ParseMatchup(week, m, teams));
        }

        private Matchup ParseMatchup(int week, string matchup, IEnumerable<Team> teams)
        {
            var values = matchup.Split('-');
            return new Matchup
            {
                Week = week,
                TeamA = teams.Single(t => t.Id == values[0]),
                TeamB = teams.Single(t => t.Id == values[1])
            };
        }
    }
}
