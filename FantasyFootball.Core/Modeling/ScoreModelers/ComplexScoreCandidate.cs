using FantasyFootball.Core.Analysis;
using FantasyFootball.Core.Modeling.ScoreModelers.ComplexCandidate;
using FantasyFootball.Core.Objects;

namespace FantasyFootball.Core.Modeling.ScoreModelers
{
    public class ComplexScoreCandidate : Candidate
    {
        private readonly HistoricalDataFilter dataFilter;
        private readonly HistoricalGroupSpecifier groupSpecifier;
        private readonly FooModel fooModel;

        public ComplexScoreCandidate(HistoricalDataFilter dataFilter, HistoricalGroupSpecifier groupSpecifier, FooModel fooModel)
        {
            this.dataFilter = dataFilter;
            this.groupSpecifier = groupSpecifier;
            this.fooModel = fooModel;
            this.Name = $"{dataFilter.Name} - {groupSpecifier.Name} - {fooModel.Name}";
            this.GetFunction = MyGetFunction;
            this.CanBeTestedOn = s => fooModel.CanPredict(s);
        }

        private ProbabilityDistribution MyGetFunction(Situation situation)
        {
            return new SampleProbability(fooModel.GetSituationValues(GetHistory(situation.Player), situation));
        }

        protected IEnumerable<Situation> GetHistoricalSituations()
        {
            return Players.All().CrossJoin(Enumerable.Range(1, SeasonWeek.ChampionshipWeek),
                (p, w) => new Situation(p.Id, w)).Where(CanBeTestedOn);
        }

        private Dictionary<string, double[]> historicalRelative;
        private double[] GetHistory(string player)
        {
            if (historicalRelative == null)
            {
                historicalRelative = GetHistoricalSituations()
                    .GroupBy(s => groupSpecifier.GetHistoricalGroup(s.Player), s => fooModel.GetHistoricalValue(s))
                    .ToDictionary(g => g.Key, g => g.ToArray());
            }
            return historicalRelative[groupSpecifier.GetHistoricalGroup(player)];
        }
    }
}
