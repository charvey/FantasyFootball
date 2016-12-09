namespace FantasyFootball.Core.Maths.BandwidthSelectors
{
    public class ConstantBandwidth : BandwidthSelector
    {
        public double SelectBandwidth(double[] x) => 1;
    }
}
