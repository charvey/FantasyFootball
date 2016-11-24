using FantasyFootball.Core.Objects;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Draft
{
    public class DraftFileEntry
    {
        public int[] DraftOrder { get; set; }
        public DraftPickEntry[] Picks { get; set; }
    }

    public class DraftPickEntry
    {
        public int TeamId { get; set; }
        public string PlayerId { get; set; }
        public int Round { get; set; }
    }

    public class DraftPickKey
    {
        public Team Team { get; set; }
        public int Round { get; set; }

        public override bool Equals(object obj)
        {
            var otherDraftPick = obj as DraftPickKey;
            if (otherDraftPick == null) return false;
            return otherDraftPick.Team.Id == this.Team.Id && otherDraftPick.Round == this.Round;
        }

        public override int GetHashCode()
        {
            return Tuple.Create(Team.Id, Round).GetHashCode();
        }
    }

    public class Draft
    {
        public List<Team> Teams { get; set; }

        private Dictionary<DraftPickKey, Player> picks = new Dictionary<DraftPickKey, Player>();

        public Player Pick(Team t, int r)
        {
            var key = GetKey(t, r);
            return picks.ContainsKey(key) ? picks[key] : null;
        }

        public void Pick(Team t, int r, Player p)
        {
            var key = GetKey(t, r);
            picks[key] = p;
        }

        public IEnumerable<Player> PickedPlayersByTeam(Team team)
        {
            return picks.Where(k => k.Key.Team.Id == team.Id).Select(x => x.Value);
        }

        private static DraftPickKey GetKey(Team t, int r) => new DraftPickKey { Team = t, Round = r };

        public IEnumerable<Player> PickedPlayers
        {
            get { return this.picks.Values; }
        }

        public static Draft FromFile()
        {
            var json = File.ReadAllText("draft.json");
            var file = JsonConvert.DeserializeObject<DraftFileEntry>(json);
            var draft= new Draft
            {
                Teams = file.DraftOrder.Select(Objects.Teams.Get).ToList()
            };
            foreach (var p in file.Picks)
                draft.Pick(Objects.Teams.Get(p.TeamId), p.Round, Players.Get(p.PlayerId));

            return draft;
        }

        public void ToFile()
        {
            var file = new DraftFileEntry
            {
                DraftOrder = this.Teams.Select(t => t.Id).ToArray(),
                Picks = this.picks.Select(p => new DraftPickEntry { TeamId = p.Key.Team.Id, PlayerId = p.Value.Id, Round = p.Key.Round }).ToArray()
            };
            var json = JsonConvert.SerializeObject(file, Formatting.Indented);
            File.WriteAllText("draft.json", json);
        }
    }
}
