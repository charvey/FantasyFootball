using System;

namespace FantasyFootball.Core.Maths.Kernels
{
    public class UniformKernel : Kernel
    {
        public double K(double u)
        {
            if (Math.Abs(u) <= 1)
                return 0.5;
            else
                return 0;
        }
    }
}
