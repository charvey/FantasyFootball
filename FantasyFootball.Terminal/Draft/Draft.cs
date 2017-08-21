using FantasyFootball.Core.Objects;
using System.Collections.Generic;

namespace FantasyFootball.Terminal.Draft
{
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
