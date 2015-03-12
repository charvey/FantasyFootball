using System.Collections.Generic;
using System.IO;
using System.Linq;
using Objects.Fantasy;

namespace Data.Csv
{
    public class LeagueRepo : ILeagueRepo
    {
        private List<League> _leagues;
        public IEnumerable<League> GetLeagues()
        {
            if (_leagues == null)
            {
                var dataset = DataSetCsvReaderWriter.fromCSV(Path.Combine(Config.DIR, "leagues.csv"));
                _leagues = dataset.Rows.Select(d => new League
                {
                    Id = d["Id"],
                    Name = d["Name"]
                }).ToList();
            }

            return _leagues;
        }

        public League GetLeague(string id)
        {
            return GetLeagues().Single(l => l.Id == id);
        }
    }
}
