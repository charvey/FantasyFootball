namespace FantasyPros.Projections
{
    public class TeProjection : Projection
    {
        public float Receptions { get; internal set; }
        public float ReceivingYards { get; internal set; }
        public float ReceivingTouchdowns { get; internal set; }
        public float Fumbles { get; internal set; }
        public float FantasyPoints { get; internal set; }
    }
}
