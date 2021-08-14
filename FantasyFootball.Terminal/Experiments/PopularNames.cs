using FantasyFootball.Data.Yahoo;
using System.IO;
using System.Linq;
using Yahoo;

namespace FantasyFootball.Terminal.Experiments
{
    public class PopularNames
    {
        private readonly TextWriter output;
        private readonly FantasySportsService service;

        public PopularNames(TextWriter output, FantasySportsService service)
        {
            this.output = output;
            this.service = service;
        }

        public void Analyze(LeagueKey leagueKey)
		{
            var players = service.LeaguePlayers(leagueKey);
            var firstNames = players.Select(p => p.name.first);
            var firstNameGroups = firstNames.GroupBy(n => n);
            foreach(var g in firstNameGroups.OrderByDescending(g => g.Count()).Take(20))
            {
                output.WriteLine($"{g.Count()} {g.Key}");
            }
        }
    }
}
