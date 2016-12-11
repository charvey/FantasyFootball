namespace FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate
{
    public interface HistoricalGroupSpecifier
    {
        string Name { get; }
        string GetHistoricalGroup(string player);
    }
}
