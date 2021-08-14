using FantasyFootball.Preseason.Abstractions;

namespace FantasyFootball.Preseason.Tests;

internal class FakeOddsClient : OddsClient
{
    private readonly IEnumerable<GameOdds> odds;

    public FakeOddsClient(IEnumerable<GameOdds> odds)
    {
        this.odds = odds;
    }

    public Task<IEnumerable<GameOdds>> GetOdds() => Task.FromResult(odds);
}