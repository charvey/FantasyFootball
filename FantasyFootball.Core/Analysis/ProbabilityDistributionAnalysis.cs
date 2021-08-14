using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling.ScoreModelers;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Yahoo;

namespace FantasyFootball.Core.Analysis
{
    public class ProbabilityDistributionAnalysis
    {
        private readonly FantasySportsService fantasySportsService;
        private readonly TextWriter @out;

        public ProbabilityDistributionAnalysis(FantasySportsService fantasySportsService, TextWriter @out)
        {
            this.fantasySportsService = fantasySportsService;
            this.@out = @out;
        }

        public void Analyze(LeagueKey leagueKey)
        {
            var allSituations = AllSituations(leagueKey);
            var candidates = Candidates.GetCandidates();

            File.Delete("candidates.csv");

            foreach (var candidate in candidates)
            {
                @out.WriteLine(candidate.Name);

                var results = TestSituations(leagueKey, allSituations, candidate).ToList();

                @out.WriteLine(results.Count() + "\t" + results.Min() + "\t" + results.Average() + "\t" + results.Max());

                File.AppendAllLines("candidates.csv", new[]
                {
                    candidate.Name+","+string.Join(",",results)
                });
            }
        }

        private IEnumerable<double> TestSituations(LeagueKey leagueKey, IEnumerable<Situation> situations, Candidate candidate)
        {
            return situations.Where(candidate.CanBeTestedOn).AsParallel().Select(s => TestSituation(leagueKey, s, candidate));
        }

        private double TestSituation(LeagueKey leagueKey, Situation situation, Candidate candidate)
        {
            var comparer = new ProbabilityDistributionComparer();
            var score = fantasySportsService.LeaguePlayersWeekStats(leagueKey, situation.Week).Single(p => p.player_id.ToString() == situation.Player).player_points.total;
            var reality = new ConstantProbabilityDistribution(score);
            var function = candidate.GetFunction(situation);
            return comparer.Compare(reality, function, -10, 40, 5);
        }

        private IEnumerable<Situation> AllSituations(LeagueKey leagueKey)
        {
            return Players.All().SelectMany(p => Enumerable.Range(1, fantasySportsService.League(leagueKey).current_week).Select(w => new Situation(p.Id, w)));
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
}
