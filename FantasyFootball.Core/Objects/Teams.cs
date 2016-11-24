using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Objects
{
    public static class Teams
    {
        private static List<Team> allTeams;
        private static DateTime lastModified = DateTime.MinValue;
        public static IEnumerable<Team> All()
        {
            if (new FileInfo("teams.json").LastWriteTime > lastModified)
            {
                allTeams = JsonConvert.DeserializeObject<List<Team>>(File.ReadAllText("teams.json"));
                lastModified = new FileInfo("teams.json").LastWriteTime;
            }
            return allTeams;
        }

        public static Team Get(int id)
        {
            return All().Single(t => t.Id == id);
        }
    }
}
