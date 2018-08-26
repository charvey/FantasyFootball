using FantasyFootball.Core.Objects;

namespace FantasyFootball.Core.Data
{
    public interface IPlayerRepository
    {
        Player GetPlayer(string playerId);
    }
}