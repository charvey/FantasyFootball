using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling.ScoreModelers;
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
            var candidates = Candidates.GetCandidates();

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
}
