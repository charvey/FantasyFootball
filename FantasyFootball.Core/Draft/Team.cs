using FantasyFootball.Core.Players;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Draft
{
    public class Team
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Owner { get; set; }
        public IEnumerable<Player> Players { get; set; } = Enumerable.Empty<Player>();

        public override bool Equals(object obj)
        {
            var otherTeam = obj as Team;
            if (otherTeam == null) return false;

            return otherTeam.Id == this.Id;
        }

        private static List<Team> allTeams;
        private static DateTime lastModified = DateTime.MinValue;
        public static IEnumerable<Team> All()
        {
            if (new FileInfo("teams.json").LastWriteTime > lastModified)
            {
                allTeams = JsonConvert.DeserializeObject<List<Team>>(File.ReadAllText("teams.json"));
                lastModified = new FileInfo("data.csv").LastWriteTime;
            }
            return allTeams;
        }

        public static Team Get(int id)
        {
            return All().Single(t => t.Id == id);
        }

        [Obsolete]
        public static Team GetWithDraftPlayers(int id)
        {
            var draft = Draft.FromFile();
            var team = Get(id);
            team.Players = draft.PickedPlayersByTeam(team);
            return team;
        }
    }
}
