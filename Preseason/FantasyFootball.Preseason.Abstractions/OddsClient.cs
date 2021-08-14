using System.Collections.Generic;
using System.Threading.Tasks;

namespace FantasyFootball.Preseason.Abstractions
{
    public interface OddsClient
    {
        Task<IEnumerable<GameOdds>> GetOdds();
    }
}
