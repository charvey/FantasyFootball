using FantasyFootball.Data.Yahoo;
using System.IO;
using System.Linq;

namespace FantasyFootball.Terminal.Experiments
{
    public static class PopularNames
    {
        public static void Analyze(FantasySportsService service, TextWriter output, string league_key)
		{
            var players = service.LeaguePlayers(league_key);
            var firstNames = players.Select(p => p.name.first);
            var firstNameGroups = firstNames.GroupBy(n => n);
            foreach(var g in firstNameGroups.OrderByDescending(g => g.Count()).Take(20))
            {
                output.WriteLine($"{g.Count()} {g.Key}");
            }
        }
    }
}
