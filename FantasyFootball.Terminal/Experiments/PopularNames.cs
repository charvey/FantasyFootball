using FantasyFootball.Core.Data;
using FantasyFootball.Data.Yahoo;
using System.IO;
using System.Linq;
using Yahoo;

namespace FantasyFootball.Terminal.Experiments
{
    public static class PopularNames
    {
        public static void Analyze(FantasySportsService service, TextWriter output, LeagueKey leagueKey)
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
