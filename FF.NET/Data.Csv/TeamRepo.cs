using System.Collections.Generic;
using System.IO;
using System.Linq;
using Objects.Fantasy;

namespace Data.Csv
{
    public class TeamRepo : ITeamRepo
    {
        private List<Team> _teams;
        public IEnumerable<Team> GetTeams()
        {
            if (_teams == null)
            {
                var dataset = DataSetCsvReaderWriter.fromCSV(Path.Combine(Config.DIR, "teams.csv"));
                _teams = dataset.Rows.Select(d => new Team
                {
                    Id = d["Id"],
                    Name = d["Name"],
                    LeagueId = d["League"]
                }).ToList();
            }

            return _teams;
        }
    }
}
