﻿using FantasyFootball.Core.Maths.Kernels;
using Xunit;

namespace FantasyFootball.Core.Tests.Maths.Kernels
{
    public class GaussianKernelTests
    {
        [Theory]
        [InlineData(double.NegativeInfinity,0)]
        [InlineData(-10, 0)]
        [InlineData(-1, 0.24)]
        [InlineData(-0.5, 0.35)]
        [InlineData(0, 0.4)]
        [InlineData(0.5, 0.35)]
        [InlineData(1, 0.24)]
        [InlineData(10, 0)]
        [InlineData(double.PositiveInfinity, 0)]
        public void K(double u, double expectedK)
        {
            var subject = new GaussianKernel();

            var actualK = subject.K(u);

            Assert.Equal(expectedK, actualK, 2);
        } 
    }
}
