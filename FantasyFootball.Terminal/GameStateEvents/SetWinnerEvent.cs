using FantasyFootball.Terminal.GameStateModels;

namespace FantasyFootball.Terminal.GameStateEvents
{
    public class SetWinnerEvent : GameStateEvent
    {
        public Matchup Matchup;
        public Team Team;
    }
}
