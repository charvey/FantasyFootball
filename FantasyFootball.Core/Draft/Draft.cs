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

    public interface Draft
    {
        IReadOnlyList<DraftParticipant> Participants { get; }
        Player Pick(DraftParticipant t, int r);
        void Pick(DraftParticipant t, int r, Player p);
        IReadOnlyList<Player> AllPlayers { get; }
        IReadOnlyList<Player> PickedPlayers { get; }
        IReadOnlyList<Player> PickedPlayersByParticipant(DraftParticipant t);
        IReadOnlyList<Player> UnpickedPlayers { get; }
    }

    public class DraftParticipant
    {
        public string Id;
        public string Name;
        public string Owner;
        public int Order;
    }
}
