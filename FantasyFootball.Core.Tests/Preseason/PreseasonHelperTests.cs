using FantasyFootball.Core.Preseason;
using Xunit;

namespace FantasyFootball.Core.Tests.Preseason
{
    public class PreseasonHelperTests
    {
        [Fact]
        public void GetAllPossibleFutures_CorrectCount()
        {
            var subject = new PreseasonHelper();

            var futures = subject.GetAllPossibleFutures();

            Assert.True(futures.Count() == 512);
        }
    }
}
