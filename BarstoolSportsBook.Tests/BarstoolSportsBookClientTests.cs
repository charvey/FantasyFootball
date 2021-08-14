using FluentAssertions;
using Xunit;

namespace BarstoolSportsBook.Tests;
public class BarstoolSportsBookClientTests
{
    [Fact]
    public async void AllGamesAreAvailable()
    {
        var subject = new BarstoolSportsBookClient(new FakeHttpClient(File.ReadAllText("Sample.json")));

        var resonse = await subject.Get("american_football/nfl_preseason");

        resonse.Events.Should().HaveCount(16);
    }
}
