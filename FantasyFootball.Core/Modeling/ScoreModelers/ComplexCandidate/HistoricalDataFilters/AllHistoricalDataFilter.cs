using FantasyFootball.Core.Analysis;

namespace FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.HistoricalDataFilters
{
    public class AllHistoricalDataFilter : HistoricalDataFilter
    {
        public string Name => "All Historical";
        public bool Include(Situation situation) => true;
    }
}
