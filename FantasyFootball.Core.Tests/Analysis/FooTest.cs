using FantasyFootball.Core.Analysis;
using Xunit;

namespace FantasyFootball.Core.Tests.Analysis
{
    public class FooTest
    {
        [Theory]
        [InlineData(5, 0.01)]
        [InlineData(5, 0.50)]
        [InlineData(5, 0.99)]
        public void ConstantInverse(double x, double percent)
        {
            var constantDistribution = new ConstantProbabilityDistribution(x);

            var result = constantDistribution.Inverse(percent);

            Assert.Equal(x, result, 3);
        }
    }
}
