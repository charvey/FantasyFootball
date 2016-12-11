using FantasyFootball.Core.Analysis;

namespace FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate
{
    public interface HistoricalDataFilter
    {
        string Name { get; }
        bool Include(Situation situation);
    }
}
