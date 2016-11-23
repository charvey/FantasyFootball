using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Analysis
{
    public abstract class CandidateEvaluator
    {
        public void EvaluateAll(TextWriter @out, IEnumerable<Candidate> candidates)
        {
            foreach(var candidate in candidates)
            {
                @out.WriteLine(candidate.Name + "\t" + Evaluate(candidate));
            }
        }

        public Candidate FindBest(IEnumerable<Candidate> candidates)
        {
            return candidates.OrderByDescending(Evaluate).First();
        }

        public abstract double Evaluate(Candidate candidate);
    }

    public class MatchupWinnerAccuracyCandidateEvaluator : CandidateEvaluator
    {
        public override double Evaluate(Candidate candidate)
        {
            var matchups = Matchups();
            return 1.0 * matchups.Count(m => CorrectPick(m, candidate)) / matchups.Count();
        }

        private bool CorrectPick(Matchup matchup, Candidate candidate)
        {
            var teamAScore = GetTeamScore(GetSituations(matchup.TeamA, matchup.Week), candidate);
            var teamBScore = GetTeamScore(GetSituations(matchup.TeamA, matchup.Week), candidate);

            if (teamAScore > teamBScore)
                return matchup.Winner == 'A';
            else if (teamAScore < teamBScore)
                return matchup.Winner == 'B';
            throw new InvalidOperationException();
        }

        private static IEnumerable<Situation> GetSituations(string[] players, int week)
        {
            return players.Select(p => new Situation(p, week));
        }

        private double GetTeamScore(IEnumerable<Situation> situations, Candidate candidate)
        {
            var predictable = situations.Where(candidate.CanBeTestedOn);
            var other = situations.Where(s => !candidate.CanBeTestedOn(s));
            var reality = new RealityCandidate();

            var functions = predictable.Select(s => candidate.GetFunction(s))
                .Concat(other.Select(s => reality.GetFunction(s)));

            return functions.Select(f => f.Inverse(0.5)).Sum();
        }

        private IEnumerable<Matchup> Matchups()
        {
            throw new NotImplementedException();
        }
    }

    public class Matchup
    {
        public string[] TeamA { get; set; }
        public string[] TeamB { get; set; }
        public int Week { get; set; }
        public char Winner { get; set; }
    }
}
