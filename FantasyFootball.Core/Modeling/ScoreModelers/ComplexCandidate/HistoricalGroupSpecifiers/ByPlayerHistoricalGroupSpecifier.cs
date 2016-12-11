namespace FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.HistoricalGroupSpecifiers
{
    public class ByPlayerHistoricalGroupSpecifier : HistoricalGroupSpecifier
    {
        public string Name => "By Player";
        public string GetHistoricalGroup(string player) => player;
    }
}
