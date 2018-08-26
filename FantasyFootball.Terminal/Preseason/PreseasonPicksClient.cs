using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Terminal.Preseason
{
    public class PreseasonPicksClient
    {
        public class Pick
        {
            public int Year;
            public int Week;
            public string Player;
            public string Team;
            public int PointsBid;
        }

        private readonly string dataDirectory;

        public PreseasonPicksClient(string dataDirectory)
        {
            this.dataDirectory = dataDirectory;
        }

        public IReadOnlyList<Pick> Get(int year, int week)
        {
            var data = File.ReadAllLines(Path.Combine(dataDirectory, $@"Preseason Picks\Preseason Picks {year} - Week {week}.csv"))
                .Select(l => l.Split(',')).ToArray();
            var playerLocations = new[]
            {
                new[]{02,8},new[]{02,12},new[]{02,16},new[]{02,20},
                new[]{21,8},new[]{21,12},new[]{21,16},new[]{21,20},
                new[]{40,8},new[]{40,12},new[]{40,16},new[]{40,20}
            };
            return playerLocations.SelectMany(xy => Enumerable.Range(0, 16)
                .Select(i => new Pick
                {
                    Year = year,
                    Week = week,
                    Player = data[xy[0]][xy[1]],
                    Team = data[xy[0] + i + 1][xy[1]],
                    PointsBid = int.Parse(data[xy[0] + i + 1][xy[1] + 1])
                }).Where(p => !string.IsNullOrWhiteSpace(p.Team))
            ).ToList();
        }
    }
}
