using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyFootball.Core.Players
{
    interface PlayerProvider
    {
        IEnumerable<Player> GetPlayers();
    }
}
