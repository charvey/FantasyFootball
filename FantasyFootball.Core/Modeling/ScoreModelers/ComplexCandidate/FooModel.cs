using FantasyFootball.Core.Analysis;

namespace FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate
{
    public interface FooModel
    {
        string Name { get; }
        bool CanPredict(Situation situation);
        double GetHistoricalValue(Situation situation);
        double[] GetSituationValues(double[] historicalData, Situation situation);
    }
}
