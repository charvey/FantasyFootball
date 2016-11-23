using System.Linq;
using FantasyFootball.Core.Analysis.WinSeason;
using Xunit;

namespace FantasyFootball.Core.Tests.Analysis.WinSeason
{    
    public class RecordTests
    {
        [Fact]
        public void WinsAreBest()
        {
            var records = new[]
            {
                new StandingsExtensions.Record(0,1,0),
                new StandingsExtensions.Record(1,0,0),
                new StandingsExtensions.Record(0,0,1)
            };

            var result = records.OrderByDescending(r => r);

            Assert.Equal(1, result.First().Wins);
        }

        [Fact]
        public void LossesAreWorst()
        {
            var records = new[]
            {
                new StandingsExtensions.Record(1,0,0),
                new StandingsExtensions.Record(0,1,0),                
                new StandingsExtensions.Record(0,0,1)
            };

            var result = records.OrderByDescending(r => r);

            Assert.Equal(1, result.Last().Losses);
        }

        [Fact]
        public void TieBetterThanLoss()
        {
            var records = new[]
            {
                new StandingsExtensions.Record(1,1,0),
                new StandingsExtensions.Record(1,0,1)
            };

            var result = records.OrderByDescending(r => r);

            Assert.Equal(1, result.First().Ties);
            Assert.Equal(1, result.Last().Losses);
        }
    }
}
