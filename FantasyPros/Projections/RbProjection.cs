namespace FantasyPros.Projections
{
    public class RbProjection : Projection
    {
        public float RushingAttempts { get; internal set; }
        public float RushingYards { get; internal set; }
        public float RushingTouchdowns { get; internal set; }
        public float Receptions { get; internal set; }
        public float ReceivingYards { get; internal set; }
        public float ReceivingTouchdowns { get; internal set; }
        public float Fumbles { get; internal set; }
        public float FantasyPoints { get; internal set; }
    }
}
