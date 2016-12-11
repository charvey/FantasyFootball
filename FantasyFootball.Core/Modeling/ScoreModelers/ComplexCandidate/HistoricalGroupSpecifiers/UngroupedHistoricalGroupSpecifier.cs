namespace FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.HistoricalGroupSpecifiers
{
    public class UngroupedHistoricalGroupSpecifier : HistoricalGroupSpecifier
    {
        public string Name => "Ungrouped";
        public string GetHistoricalGroup(string player) => string.Empty;
    }
}
