using HtmlAgilityPack;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

namespace FantasyFootball.Terminal.Daily
{
    public class DailyModel3
    {
        private readonly TextWriter output;
        private readonly string dataDirectory;
        private readonly SQLiteConnection connection;
        private readonly FantasyPros fantasyPros;

        private Dictionary<string, Normal> points;
        private float threshold;

        public DailyModel3(SQLiteConnection connection, TextWriter output, string dataDirectory)
        {
            this.connection = connection;
            this.output = output;
            this.dataDirectory = dataDirectory;
            this.fantasyPros = new FantasyPros(dataDirectory);
        }

        static Normal Combine(Normal a, Normal b)
        {
            return new Normal(a.Mean + b.Mean, Math.Sqrt(a.Variance + b.Variance));
        }

        static Normal Multiply(Normal x, float m)
        {
            return new Normal(x.Mean * m, x.StdDev * Math.Abs(m));
        }

        Normal ExpectedPoints(DailyPlayer player, DateTime at)
        {
            var row = fantasyPros.GetPlayerRow(player, at);

            if (row == null)
            {
                output.WriteLine($"Can't find {player.Name} {player.Salary:C}");
                return new Normal(0, 0);
            }

            switch (player.Position)
            {
                case "QB": return ParseForQB(row);
                case "WR": return ParseForWR(row);
                case "RB": return ParseForRB(row);
                case "TE": return ParseForTE(row);
                case "DEF": return ParseForDST(row);
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private static Normal ParseForQB(HtmlNode row)
        {
            var tds = row.Elements("td").ToArray();
            var outcome = new Normal(0, 0);
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[3].InnerText)), 0.04f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[4].InnerText)), 4f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[5].InnerText)), -1f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[7].InnerText)), 0.1f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[8].InnerText)), 6f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[9].InnerText)), -2f));
            return outcome;
        }

        private static Normal ParseForWR(HtmlNode row)
        {
            var tds = row.Elements("td").ToArray();
            var outcome = new Normal(0, 0);
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[2].InnerText)), 0.1f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[3].InnerText)), 6.0f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[4].InnerText)), 0.5f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[5].InnerText)), 0.1f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[6].InnerText)), 6.0f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[7].InnerText)), -2f));
            return outcome;
        }

        private static Normal ParseForRB(HtmlNode row)
        {
            var tds = row.Elements("td").ToArray();
            var outcome = new Normal(0, 0);
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[2].InnerText)), 0.1f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[3].InnerText)), 6.0f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[4].InnerText)), 0.5f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[5].InnerText)), 0.1f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[6].InnerText)), 6.0f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[7].InnerText)), -2f));
            return outcome;
        }

        private static Normal ParseForTE(HtmlNode row)
        {
            var tds = row.Elements("td").ToArray();
            var outcome = new Normal(0, 0);
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[1].InnerText)), 0.5f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[2].InnerText)), 0.1f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[3].InnerText)), 6.0f));
            outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[4].InnerText)), -2f));
            return outcome;
        }

        private static Normal ParseForDST(HtmlNode row)
        {
            return Estimate(float.Parse(row.Elements("td").ToArray()[10].InnerText));
        }

        private static Normal Estimate(float mean)
        {
            return new Normal(mean, Math.Sqrt(mean));
        }

        private class LineupEqualityComparer : IEqualityComparer<DailyPlayer[]>
        {
            public bool Equals(DailyPlayer[] x, DailyPlayer[] y) => ConcatIds(x) == ConcatIds(y);
            public int GetHashCode(DailyPlayer[] obj) => ConcatIds(obj).GetHashCode();
            private string ConcatIds(DailyPlayer[] players) => string.Join(":", players.OrderBy(p => p.Id).Select(p => p.Id));
        }

        public void Do(int contestId)
        {
            var contest = DailyFantasyService.GetContest(contestId);
            var budget = contest.salaryCap;
            var startTime = new DateTime(1970, 1, 1).AddMilliseconds(contest.startTime);

            var sw = Stopwatch.StartNew();

            var players = DailyFantasyService.GetPlayers(contestId).ToArray();
            var playerLookup = players.ToDictionary(p => p.Id);

            output.WriteLine($"{sw.Elapsed} {players.Length} players eligible");
            points = players.ToDictionary(p => p.Id, p => ExpectedPoints(p, startTime));
            threshold = 100f;

            {
                var pointLine = 2;
                var chance = 0.6;
                players = players.Where(player => player.Position == "DEF" || points[player.Id].CumulativeDistribution(pointLine) < (1 - chance)).ToArray();
                output.WriteLine($"{sw.Elapsed} {players.Length} players with {chance:P} chance to get {pointLine} points");
            }

            var done = false;
            var queue = new ConcurrentQueue<DailyPlayer[]>();
            var qualified = new ConcurrentDictionary<DailyPlayer[], double>();
            var processed = 0L;
            var qualifiedCount = 0L;
            var producer = new LineupGeneratorProducer(players, budget);

            new Thread(() =>
             {
                 producer.Start(queue);
                 done = true;
             }).Start();

            var consumers = new Thread[10];
            for (var i = 0; i < consumers.Length; i++)
            {
                consumers[i] = new Thread(() =>
                {
                    while (!done || !queue.IsEmpty)
                    {
                        DailyPlayer[] lineup;
                        if (queue.TryDequeue(out lineup))
                        {
                            var chanceToCash = ChanceToCash(lineup);
                            if (chanceToCash > 0.6f)
                            {
                                if (!qualified.TryAdd(lineup, chanceToCash))
                                    throw new InvalidOperationException();
                                Interlocked.Increment(ref qualifiedCount);
                            }
                            Interlocked.Increment(ref processed);
                        }
                        else
                        {
                            Thread.Sleep(TimeSpan.FromSeconds(0.5));
                        }
                    }
                });
            }

            foreach (var consumer in consumers)
                consumer.Start();
            while (!done || !queue.IsEmpty)
            {
                output.WriteLine($"{sw.Elapsed} {queue.Count} {qualifiedCount}/{processed} {producer.Done}/{producer.Total} ({1.0 * producer.Done / producer.Total:P})");
                if (qualified.Any())
                {
                    var ordered = qualified.ToArray().OrderByDescending(l => l.Value);
                    var best = ordered.First();
                    DisplayInfo(best.Key);
                    foreach(var x in ordered.Skip(1000))
                    {
                        if (!qualified.TryRemove(x.Key, out double _))
                            throw new Exception();
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            foreach (var consumer in consumers)
                consumer.Join();

            var lineups = qualified.Keys.Distinct(new LineupEqualityComparer()).ToArray();

            output.WriteLine($"{lineups.Count()} lineups at least {threshold} points");

            lineups = lineups.OrderByDescending(l => ChanceToCash(l)).ToArray();

            foreach (var lineup in lineups.Take(20))
            {
                DisplayInfo(lineup);
            }
        }

        private double ChanceToCash(DailyPlayer[] lineup)
        {
            var mean = 0.0;
            var totalVariance = 0.0;
            foreach (var player in lineup)
            {
                var playerPoints = points[player.Id];
                mean += playerPoints.Mean;
                totalVariance += playerPoints.Variance;
            }
            return 1 - new Normal(mean, Math.Sqrt(totalVariance)).CumulativeDistribution(threshold);
        }

        private void DisplayInfo(DailyPlayer[] lineup)
        {
            var orderedLineup = lineup.OrderBy(p => p.Position).ThenBy(p => p.Name);

            output.WriteLine(string.Join(" ", new[]{
                        $"Chace to Cash: {ChanceToCash(lineup):P}",
                        $"Salary: ${lineup.Sum(p => p.Salary)}"
                    }));
            output.WriteLine($"\t{string.Join(",", orderedLineup.Skip(0).Take(3).Select(p => p.Name + " " + p.Position))}");
            output.WriteLine($"\t{string.Join(",", orderedLineup.Skip(3).Take(3).Select(p => p.Name + " " + p.Position))}");
            output.WriteLine($"\t{string.Join(",", orderedLineup.Skip(6).Take(3).Select(p => p.Name + " " + p.Position))}");
        }
    }
}
