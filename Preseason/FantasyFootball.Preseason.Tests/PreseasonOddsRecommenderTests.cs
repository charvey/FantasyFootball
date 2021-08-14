using FantasyFootball.Preseason.Abstractions;
using FluentAssertions;
using Xunit;

namespace FantasyFootball.Preseason.Tests;
public class PreseasonOddsRecommenderTests
{
    [Fact]
    public void FavoriteIsRecommended()
    {
        var odds = new FakeOddsClient(new List<GameOdds>
        {
            new GameOdds(new TeamOdds[]{
                new TeamOdds("Eagles",1.00,-5,1.00),
                new TeamOdds("Patriots",2.00,-5,1.00)
            })
        });

        var subject = new PreseasonOddsRecommender(odds);

        subject.GetRecommendations().Should().ContainSingle()
            .Which.Equals(new PreseasonOddsRecommender.Recommendation("Eagles"));
    }

    [Fact]
    public void EvenMoneyFallsBackToSpread()
    {
        var odds = new FakeOddsClient(new[]
        {
            new GameOdds(new[]
            {
                new TeamOdds("Patriots",1.95,+0.5m,1.00),
                new TeamOdds("Eagles",1.95,-0.5m,1.00)
            })
        });

        var subject = new PreseasonOddsRecommender(odds);

        subject.GetRecommendations().Should().ContainSingle()
            .Which.Equals(new PreseasonOddsRecommender.Recommendation("Eagles"));
    }

    [Fact]
    public void EvenSpreadFallsBackToSpreadOdds()
    {
        var odds = new FakeOddsClient(new[]
        {
            new GameOdds(new[]
            {
                new TeamOdds("Eagles",1.95,0m,1.90),
                new TeamOdds("Patriots",1.95,0m,1.95)
            })
        });

        var subject = new PreseasonOddsRecommender(odds);

        subject.GetRecommendations().Should().ContainSingle()
            .Which.Equals(new PreseasonOddsRecommender.Recommendation("Eagles"));
    }

    [Fact]
    public void BiggestFavoriteIsFirst()
    {
        var odds = new FakeOddsClient(new[]
        {
            new GameOdds(new[]
            {
                new TeamOdds("Ravens",1.05,+0.5m,1.00),
                new TeamOdds("Steelers",1.95,-0.5m,1.00)
            }),
            new GameOdds(new[]
            {
                new TeamOdds("Patriots",2.00,+0.5m,1.00),
                new TeamOdds("Eagles",1.00,-0.5m,1.00)
            })
        });

        var subject = new PreseasonOddsRecommender(odds);

        subject.GetRecommendations().Should().Equal(new[]{
                new PreseasonOddsRecommender.Recommendation("Eagles"),
                new PreseasonOddsRecommender.Recommendation("Ravens")
            });
    }

    [Fact]
    public void BiggestSpreadBreakTies()
    {
        var odds = new FakeOddsClient(new[]
        {
            new GameOdds(new[]
            {
                new TeamOdds("Steelers",1.95,+0.5m,1.00),
                new TeamOdds("Ravens",1.95,-0.5m,1.00)
            }),
            new GameOdds(new[]
            {
                new TeamOdds("Patriots",1.95,+1.5m,1.00),
                new TeamOdds("Eagles",1.95,-1.5m,1.00)
            })
        });

        var subject = new PreseasonOddsRecommender(odds);

        subject.GetRecommendations().Should().Equal(new[]{
                new PreseasonOddsRecommender.Recommendation("Eagles"),
                new PreseasonOddsRecommender.Recommendation("Ravens")
            });
    }

    [Fact]
    public void BestSpreadOddsBreakTies()
    {
        var odds = new FakeOddsClient(new[]
        {
            new GameOdds(new[]
            {
                new TeamOdds("Ravens",1.95,0m,1.90),
                new TeamOdds("Steelers",1.95,0m,1.95)
            }),
            new GameOdds(new[]
            {
                new TeamOdds("Patriots",1.95,0m,2.00),
                new TeamOdds("Eagles",1.95,0m,1.00)
            })
        });

        var subject = new PreseasonOddsRecommender(odds);

        subject.GetRecommendations().Should().Equal(new[]{
                new PreseasonOddsRecommender.Recommendation("Eagles"),
                new PreseasonOddsRecommender.Recommendation("Ravens")
            });
    }
}
