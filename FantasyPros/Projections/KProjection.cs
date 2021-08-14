namespace FantasyPros.Projections
{
    public class KProjection : Projection
    {
        public float FieldGoals { get; internal set; }
        public float FieldGoalAttempts { get; internal set; }
        public float ExtraPoints { get; internal set; }
        public float FantasyPoints { get; internal set; }
    }
}
