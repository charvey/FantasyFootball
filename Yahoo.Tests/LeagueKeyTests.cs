using System;
using System.Collections.Generic;
using Xunit;

namespace Yahoo.Tests
{
    public class LeagueKeyTests
    {
        [Theory]
        [InlineData("123.l.56789")]
        public void Parse_ValidFormats(string value)
        {
            Assert.IsType<LeagueKey>(LeagueKey.Parse(value));
        }

        [Theory]
        [InlineData("123.l.56789.t.1")]
        [InlineData("123.l.56789.p.1")]
        [InlineData("56789")]
        [InlineData("123.56789")]
        public void Parse_InvalidFormats(string value)
        {
            Assert.Throws<FormatException>(() => LeagueKey.Parse(value));
        }

        [Theory]
        [InlineData("123.l.56789")]
        public void ParseToString_RoundTrips(string value)
        {
            Assert.Equal(value, LeagueKey.Parse(value).ToString());
        }

        [Theory]
        [InlineData("123.l.56789")]
        public void UsableInCollections(string value)
        {
            var collection = new HashSet<LeagueKey> { LeagueKey.Parse(value) };

            Assert.Contains(LeagueKey.Parse(value), collection);
        }

        [Theory]        
        [InlineData("123.l.12345", 12345)]
        [InlineData("123.l.67890", 67890)]
        public void LeagueId_ParsedCorrectly(string value, int leagueId)
        {
            Assert.Equal(leagueId, LeagueKey.Parse(value).LeagueId);
        }
    }
}
