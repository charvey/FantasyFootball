using FantasyFootball.Core.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Data
{
    public static class Scores
    {
        public static double GetTotalScore(Player p)
        {
            return GetScores(p).Sum();
        }

        public static double GetScore(Player p, int week)
        {
            return GetScores(p)[week - 1];
        }

        private static double[] GetScores(Player p)
        {
            return GetAllScores()[p.Id];
        }

        private static Dictionary<string, double[]> scores = new Dictionary<string, double[]>();
        private static DateTime lastModified = DateTime.MinValue;
        private static IReadOnlyDictionary<string, double[]> GetAllScores()
        {
            if (new FileInfo("data.csv").LastWriteTime > lastModified)
            {
                scores = File.ReadAllLines("data.csv").Select(l => l.Split(','))
                    .ToDictionary(l => l[0], l => l.Skip(4).Take(17).Select(double.Parse).ToArray());
                lastModified = new FileInfo("data.csv").LastWriteTime;
            }
            return scores;
        }
    }
}
