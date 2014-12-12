using Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Data
{
    public static class LeagueRepo
    {
        private static List<League> _leagues;
        public static IEnumerable<League> GetLeagues()
        {
            if (_leagues == null)
            {
                var dataset = DataSet.fromCSV(Path.Combine(Config.DIR, "leagues.csv"));
                _leagues = dataset.Rows.Select(d => new League
                {
                    Id = d["Id"],
                    Name = d["Name"]
                }).ToList();
            }

            return _leagues;
        }

        public static League GetLeague(string id)
        {
            return GetLeagues().Single(l => l.Id == id);
        }
    }
}
