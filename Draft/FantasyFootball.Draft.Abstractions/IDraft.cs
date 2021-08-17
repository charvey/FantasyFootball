using System.Collections.Generic;

namespace FantasyFootball.Draft.Abstractions
{
    public interface IDraft
    {
        IReadOnlyList<DraftParticipant> Participants { get; }
        Player Pick(DraftParticipant t, int r);
        void Pick(DraftParticipant t, int r, Player p);
        IReadOnlyList<Player> AllPlayers { get; }
        DraftParticipant ParticipantByPlayer(Player p);
        IReadOnlyList<Player> PickedPlayers { get; }
        IReadOnlyList<Player> PickedPlayersByParticipant(DraftParticipant t);
        IReadOnlyList<Player> UnpickedPlayers { get; }
    }
}
