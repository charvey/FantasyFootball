﻿using FantasyFootball.Core.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Data
{
    public static class DumpData
    {
        private const string filename = "dump.csv";

        public static double GetScore(Player p, int week)
        {
            if (week < SeasonWeek.Current)
                return GetActualScore(p.Id, week).Value;
            else
                return GetPrediction(p.Id, SeasonWeek.Current, week).Value;
        }

        public static double GetSeasonTotalScore(Player p)
        {
            return Enumerable.Range(1, SeasonWeek.Maximum).Select(w => GetScore(p, w)).Sum();
        }

        public static double? GetActualScore(string player, int week)
        {
            return GetValue(player, $"A-{week}");
        }

        public static double? GetPrediction(string player, int fromWeek, int aboutWeek)
        {
            return GetValue(player, $"P-{fromWeek}-{aboutWeek}");
        }

        private static double? GetValue(string player, string key)
        {
            return GetAllScores()[player][fieldIndexes[key]];
        }

        private static IReadOnlyDictionary<string, double?[]> scores = new Dictionary<string, double?[]>();
        private static IReadOnlyDictionary<string, int> fieldIndexes = new Dictionary<string, int>();
        private static DateTime lastModified = DateTime.MinValue;
        private static IReadOnlyDictionary<string, double?[]> GetAllScores()
        {
            if (new FileInfo(filename).LastWriteTime > lastModified)
            {
                scores = File.ReadAllLines(filename).Select(l => l.Split(','))
                    .ToDictionary(l => l[0], l => l.Skip(2).Select(NullableParse).ToArray());
                fieldIndexes = GetFieldIndexes();
                lastModified = new FileInfo(filename).LastWriteTime;
            }
            return scores;
        }

        private static IReadOnlyDictionary<string, int> GetFieldIndexes()
        {
            var lines = File.ReadLines(filename);
            var firstLine = lines.First();
            return firstLine.Split(',').Skip(2).Select((f, i) => Tuple.Create(f, i))
                   .ToDictionary(x => x.Item1, x => x.Item2);
        }

        private static double? NullableParse(string s)
        {
            double value;
            if (double.TryParse(s, out value))
                return value;
            return null;
        }
    }
}
