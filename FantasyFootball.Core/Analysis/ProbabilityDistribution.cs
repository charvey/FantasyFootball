namespace FantasyFootball.Core.Analysis
{
    public interface ProbabilityDistribution
    {
        double GetProbabilityInRange(double left, double right);
    }

    public static class Foo
    {
        public static double Inverse(this ProbabilityDistribution distribution, double probability)
        {
            double left = -1e6;
            double right = 1e6;
            double x = double.NaN;

            while ((right - left) > 0.001)
            {
                x = (right + left) / 2;

                var currentProbability = distribution.GetProbabilityInRange(double.NegativeInfinity, x);
                if (currentProbability < probability)
                    left = x;
                else
                    right = x;
            }

            return x;
        }
    }

    public class ConstantProbabilityDistribution : ProbabilityDistribution
    {
        private readonly double constantValue;

        public ConstantProbabilityDistribution(double constantValue)
        {
            this.constantValue = constantValue;
        }

        public double GetProbabilityInRange(double left, double right)
        {
            return left <= constantValue && constantValue <= right ? 1 : 0;
        }
    }

    public class SampleProbability : ProbabilityDistribution
    {
        private readonly double[] samples;

        public SampleProbability(double[] samples)
        {
            if (samples.Any(double.IsNaN)) throw new ArgumentException("Samples must be numbers", nameof(samples));
            this.samples = samples;
        }

        public double GetProbabilityInRange(double left, double right)
        {
            var inRange = samples.Count(x => left <= x && x <= right);
            return 1.0 * inRange / samples.Length;
        }
    }

    public class ProbabilityDistributionComparer
    {
        public double Compare(ProbabilityDistribution a, ProbabilityDistribution b, double left, double right, double width)
        {
            if (left > right) throw new ArgumentException("Range must be correct");
            if ((right - left) % 1 != 0) throw new ArgumentException("Must be even fit", nameof(width));

            var sum = 0.0;
            for (double l = left; l < right; l += width)
            {
                var r = l + width;
                var difference = a.GetProbabilityInRange(l, r) - b.GetProbabilityInRange(l, r);
                sum += Math.Abs(difference);
            }
            var periods = (right - left) / width;

            return sum / periods;
        }
    }
}
