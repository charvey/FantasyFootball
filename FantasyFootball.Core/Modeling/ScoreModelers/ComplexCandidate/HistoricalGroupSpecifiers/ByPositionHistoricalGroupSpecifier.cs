namespace FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.HistoricalGroupSpecifiers
{
    public class ByPositionHistoricalGroupSpecifier : HistoricalGroupSpecifier
    {
        private readonly Func<string, string[]> playerPositions;

        public ByPositionHistoricalGroupSpecifier(Func<string, string[]> playerPositions)
        {
            this.playerPositions = playerPositions;
        }

        public string Name => "By Position";
        public string GetHistoricalGroup(string player) => string.Join("/", playerPositions(player));
    }
}
