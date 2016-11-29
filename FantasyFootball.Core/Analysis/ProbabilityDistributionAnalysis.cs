using FantasyFootball.Core.Data;
using FantasyFootball.Core.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Analysis
{
    public class ProbabilityDistributionAnalysis
    {
        public static int CurrentWeek = SeasonWeek.Current;

        public static void Analyze(TextWriter @out)
        {
            var allSituations = AllSituations();
            var testableSituations = allSituations.Where(CanBeEvaluated);
            var candidates = GetCandidates();

            File.Delete("candidates.csv");

            foreach (var candidate in candidates)
            {
                @out.WriteLine(candidate.Name);

                var results = TestSituations(testableSituations, candidate).ToList();

                @out.WriteLine(results.Count() + "\t" + results.Min() + "\t" + results.Average() + "\t" + results.Max());

                File.AppendAllLines("candidates.csv", new[]
                {
                    candidate.Name+","+string.Join(",",results)
                });
            }
        }

        private static IEnumerable<double> TestSituations(IEnumerable<Situation> situations, Candidate candidate)
        {
            return situations.Where(candidate.CanBeTestedOn).AsParallel().Select(s => TestSituation(s, candidate));
        }

        private static double TestSituation(Situation situation, Candidate candidate)
        {
            var comparer = new ProbabilityDistributionComparer();
            var reality = new ConstantProbabilityDistribution(DumpData.GetActualScore(situation.Player, situation.Week).Value);
            var function = candidate.GetFunction(situation);
            return comparer.Compare(reality, function, -10, 40, 5);
        }

        private static IEnumerable<Situation> AllSituations()
        {
            return Players.All().SelectMany(p => Enumerable.Range(1, CurrentWeek).Select(w => new Situation(p.Id, w)));
        }

        private static bool CanBeEvaluated(Situation situation) => DumpData.GetActualScore(situation.Player, situation.Week).HasValue;

        private static IEnumerable<Candidate> GetCandidates()
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
                new UngroupedHistoricalGroupSpecifier(),new ByPlayerHistoricalGroupSpecifier(),new ByPositionHistoricalGroupSpecifier()
            }, new FooModel[] { new AdjustedByYahooPredictionModel(), new RawScoreModel() }
            , (h, g, m) => new ComplexCandidate(h, g, m));

            foreach (var complexCandidate in complexCandidates)
                yield return complexCandidate;
        }
    }

    public class Situation
    {
        public string Player { get; set; }
        public int Week { get; set; }

        public Situation(string player, int week) { this.Player = player; this.Week = week; }
    }

    public class Candidate
    {
        public string Name { get; set; }
        public Func<Situation, ProbabilityDistribution> GetFunction { get; set; }
        public Func<Situation, bool> CanBeTestedOn { get; set; }
    }

    public class RealityCandidate : Candidate
    {
        public RealityCandidate()
        {
            Name = "Reality";
            GetFunction = s => new ConstantProbabilityDistribution(DumpData.GetActualScore(s.Player, s.Week).Value);
            CanBeTestedOn = s => DumpData.GetActualScore(s.Player, s.Week).HasValue;
        }
    }

    public class NoScoreCandidate : Candidate
    {
        public NoScoreCandidate()
        {
            this.Name = "No Score";
            this.GetFunction = s => new ConstantProbabilityDistribution(0);
            this.CanBeTestedOn = s => true;
        }
    }

    public class ComplexCandidate : Candidate
    {
        private readonly HistoricalDataFilter dataFilter;
        private readonly HistoricalGroupSpecifier groupSpecifier;
        private readonly FooModel fooModel;

        public ComplexCandidate(HistoricalDataFilter dataFilter, HistoricalGroupSpecifier groupSpecifier, FooModel fooModel)
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
            return Players.All().CrossJoin(Enumerable.Range(1, ProbabilityDistributionAnalysis.CurrentWeek),
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

    public interface HistoricalGroupSpecifier
    {
        string Name { get; }
        string GetHistoricalGroup(string player);
    }

    public class UngroupedHistoricalGroupSpecifier : HistoricalGroupSpecifier
    {
        public string Name => "Ungrouped";
        public string GetHistoricalGroup(string player) => string.Empty;
    }

    public class ByPositionHistoricalGroupSpecifier : HistoricalGroupSpecifier
    {
        public string Name => "By Position";
        public string GetHistoricalGroup(string player) => string.Join("/", Players.Get(player).Positions);
    }

    public class ByPlayerHistoricalGroupSpecifier : HistoricalGroupSpecifier
    {
        public string Name => "By Player";
        public string GetHistoricalGroup(string player) => player;
    }

    public interface FooModel
    {
        string Name { get; }
        bool CanPredict(Situation situation);
        double GetHistoricalValue(Situation situation);
        double[] GetSituationValues(double[] historicalData, Situation situation);
    }

    public class AdjustedByYahooPredictionModel : FooModel
    {
        public string Name => "Adjusted by Yahoo Prediction";

        public bool CanPredict(Situation situation)
        {
            return DumpData.GetPrediction(situation.Player, situation.Week, situation.Week).HasValue;
        }

        public double GetHistoricalValue(Situation s)
        {
            var actual = DumpData.GetActualScore(s.Player, s.Week).Value;
            var prediction = DumpData.GetPrediction(s.Player, s.Week, s.Week).Value;

            return (actual + 10) / (prediction + 10);
        }

        public double[] GetSituationValues(double[] historicalData, Situation situation)
        {
            var prediction = DumpData.GetPrediction(situation.Player, situation.Week, situation.Week).Value;

            return historicalData.Select(x => x * (prediction + 10) - 10).ToArray();
        }
    }

    public class RawScoreModel : FooModel
    {
        public string Name => "Raw Scores";

        public bool CanPredict(Situation situation)
        {
            return true;
        }

        public double GetHistoricalValue(Situation s)
        {
            return DumpData.GetActualScore(s.Player, s.Week).Value;
        }

        public double[] GetSituationValues(double[] historicalData, Situation situation)
        {
            return historicalData;
        }
    }
}
