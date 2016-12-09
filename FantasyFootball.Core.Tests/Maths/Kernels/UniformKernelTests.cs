using FantasyFootball.Core.Maths.Kernels;
using Xunit;

namespace FantasyFootball.Core.Tests.Maths.Kernels
{
    public class UniformKernelTests
    {
        [Theory]
        [InlineData(double.NegativeInfinity,0)]
        [InlineData(-10, 0)]
        [InlineData(-1, 0.5)]
        [InlineData(-0.5, 0.5)]
        [InlineData(0, 0.5)]
        [InlineData(0.5, 0.5)]
        [InlineData(1, 0.5)]
        [InlineData(10, 0)]
        [InlineData(double.PositiveInfinity, 0)]
        public void K(double u, double expectedK)
        {
            var subject = new UniformKernel();

            var actualK = subject.K(u);

            Assert.Equal(expectedK, actualK, 2);
        } 
    }
}
