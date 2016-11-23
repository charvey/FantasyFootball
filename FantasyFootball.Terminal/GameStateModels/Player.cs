using System.Collections.Generic;

namespace FantasyFootball.Terminal.GameStateModels
{
    public class Player
    {
        public string Id;
        public string Name;
        public string Team;
        public ISet<string> Positions;
    }
}
