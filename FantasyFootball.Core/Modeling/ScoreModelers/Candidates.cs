using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate;
using FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.FooModels;
using FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.HistoricalDataFilters;
using FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate.HistoricalGroupSpecifiers;

namespace FantasyFootball.Core.Modeling.ScoreModelers
{
    public static class Candidates
    {
        public static IEnumerable<Candidate> GetCandidates()
        {
            yield return new Candidate
            {
                Name = "Yahoo Prediction",
                GetFunction = s => new ConstantProbabilityDistribution(DumpData.GetPrediction(s.Player, s.Week, s.Week).Value),
                CanBeTestedOn = s => DumpData.GetPrediction(s.Player, s.Week, s.Week).HasValue
            };

            yield return new RealityCandidate();

            yield return new NoScoreCandidate();

            var complexCandidates = new HistoricalDataFilter[]
            {
                new AllHistoricalDataFilter(),
                new PredictedToScoreAtLeastHistoricalDataFilter(1),
                new PredictedToScoreAtLeastHistoricalDataFilter(2),
                new PredictedToScoreAtLeastHistoricalDataFilter(3),
                new PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter(1,0.1),
                new PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter(1,0.25),
                new PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter(1,0.33),
                new PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter(2,0.1),
                new PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter(2,0.25),
                new PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter(2,0.33),
                new PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter(3,0.1),
                new PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter(3,0.25),
                new PredictedToScoreAtLeastAndNotFlukeHistoricalDataFilter(3,0.33)
            }.CrossJoin(new HistoricalGroupSpecifier[]
            {
                new UngroupedHistoricalGroupSpecifier(),
                new ByPlayerHistoricalGroupSpecifier(),
                new ByPositionHistoricalGroupSpecifier(_=>throw new NotImplementedException())
            }, new FooModel[] { new AdjustedByYahooPredictionModel(), new RawScoreModel() }
            , (h, g, m) => new ComplexScoreCandidate(h, g, m));

            foreach (var complexCandidate in complexCandidates)
                yield return complexCandidate;
        }
    }
}
