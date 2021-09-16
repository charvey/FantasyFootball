namespace FantasyFootball.Preseason.Abstractions
{
    public interface OddsClient
    {
        Task<IEnumerable<GameOdds>> GetOdds();
    }
}
