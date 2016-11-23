using FantasyFootball.Terminal.GameStateModels;

namespace FantasyFootball.Terminal.GameStateEvents
{
    public class DraftPlayerEvent : GameStateEvent
    {
        public Team Team;
        public Player Player;
        public int Round;
    }
}
