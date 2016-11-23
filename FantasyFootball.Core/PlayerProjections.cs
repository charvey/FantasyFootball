using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyFootball.Core
{
    public interface PlayerProjections
    {
        double Get(ushort playerId, int week);
    }
}
