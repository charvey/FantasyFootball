namespace FantasyPros.Projections
{
    public class DstProjection : Projection
    {
        public float Sacks { get; internal set; }
        public float Interceptions { get; internal set; }
        public float FumbleRecovery { get; internal set; }
        public float ForcedFumble { get; internal set; }
        public float Touchdowns { get; internal set; }
        public float? Assist______ { get; internal set; }
        public float Safeties { get; internal set; }
        public float PointsAgaints { get; internal set; }
        public float YardsAgainst { get; internal set; }
        public float FantasyPoints { get; internal set; }
    }
}
