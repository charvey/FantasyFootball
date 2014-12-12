using Objects;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Data
{
    public static class TeamRepo
    {
        private static List<Team> _teams;
        public static IEnumerable<Team> GetTeams()
        {
            if (_teams == null)
            {
                var dataset = DataSet.fromCSV(Path.Combine(Config.DIR, "teams.csv"));
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
