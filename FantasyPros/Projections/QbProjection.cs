namespace FantasyPros.Projections
{
    public class QbProjection : Projection
    {
        public float PassingAttempts { get; internal set; }
        public float PassingCompletions { get; internal set; }
        public float PassingYards { get; internal set; }
        public float PassingTouchdowns { get; internal set; }
        public float Interceptions { get; internal set; }
        public float RushingAttempts { get; internal set; }
        public float RushingYards { get; internal set; }
        public float RushingTouchdowns { get; internal set; }
        public float Fumbles { get; internal set; }
        public float FantasyPoints { get; internal set; }
    }
}
