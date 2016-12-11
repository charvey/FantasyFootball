using FantasyFootball.Core.Objects;

namespace FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.HistoricalGroupSpecifiers
{
    public class ByPositionHistoricalGroupSpecifier : HistoricalGroupSpecifier
    {
        public string Name => "By Position";
        public string GetHistoricalGroup(string player) => string.Join("/", Players.Get(player).Positions);
    }
}
