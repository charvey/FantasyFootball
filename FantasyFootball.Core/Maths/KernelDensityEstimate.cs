using FantasyFootball.Core.Maths.BandwidthSelectors;
using FantasyFootball.Core.Maths.Kernels;
using System;
using System.Linq;

namespace FantasyFootball.Core.Maths
{
    class KernelDensityEstimate
    {
        private readonly double[] data;
        private readonly double h;
        private readonly Func<double, double> k;

        public KernelDensityEstimate(double[] data, BandwidthSelector bandwidthSelector, Kernel kernel)
        {
            this.data = data;
            this.h = bandwidthSelector.SelectBandwidth(data);
            this.k = kernel.K;
        }

        public double P(double x)
        {
            return data.Sum(xi => k((x - xi) / h)) / (data.Length * h);
        }
    }
}
