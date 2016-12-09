using System;

namespace FantasyFootball.Core.Maths.Kernels
{
    public class GaussianKernel : Kernel
    {
        public double K(double u)
        {
            return Math.Pow(Math.E, -0.5 * u * u) / Math.Sqrt(2 * Math.PI);
        }
    }
}
