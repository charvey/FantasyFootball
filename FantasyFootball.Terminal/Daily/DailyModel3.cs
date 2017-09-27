using HtmlAgilityPack;
using MathNet.Numerics.Distributions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Diagnostics;
using System.Linq;
using System.Threading;

namespace FantasyFootball.Terminal.Daily
{
    public class DailyModel3
    {
        private readonly string dataDirectory;
        private readonly SQLiteConnection connection;
        private readonly FantasyPros fantasyPros;

        public DailyModel3(SQLiteConnection connection, string dataDirectory)
        {
            this.connection = connection;
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

        Normal ExpectedPoints(SQLiteConnection connection, DailyPlayer player)
        {
            var row = fantasyPros.GetPlayerRow(player);

            if (row == null)
            {
                Console.WriteLine($"Can't find {player.Name} {player.Salary:C}");
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
            var budget = 200;

            var sw = Stopwatch.StartNew();

            var players = DailyFantasyService.GetPlayers(contestId).ToArray();
            var playerLookup = players.ToDictionary(p => p.Id);

            Console.WriteLine($"{sw.Elapsed} {players.Length} players eligible");
            var points = players.ToDictionary(p => p.Id, p => ExpectedPoints(connection, p));

            {
                var pointLine = 2;
                var chance = 0.6;
                players = players.Where(player => player.Position == "DEF" || points[player.Id].CumulativeDistribution(pointLine) < (1 - chance)).ToArray();
                Console.WriteLine($"{sw.Elapsed} {players.Length} players with {chance:P} chance to get {pointLine} points");
            }

            var threshold = 100f;

            var done = false;
            var queue = new ConcurrentQueue<DailyPlayer[]>();
            var qualified = new ConcurrentBag<DailyPlayer[]>();
            var processed = 0L;

            var producer = new Thread(() =>
              {
                  foreach (var lineup in LineupGenerator.GenerateLineups(players, budget).Where(l => l.Sum(p => p.Salary) <= budget))
                  {
                      queue.Enqueue(lineup);
                      if (queue.Count > 100000) Thread.Yield();
                  }
                  done = true;
              });

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
                            if (ChanceToCash(lineup, points, threshold) > 0.6f)
                            {
                                qualified.Add(lineup);
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

            producer.Start();
            foreach (var consumer in consumers)
                consumer.Start();
            while (!done || !queue.IsEmpty)
            {
                Console.WriteLine($"{sw.Elapsed} {queue.Count} {qualified.Count}/{processed}");
                if (qualified.Any())
                {
                    var best = qualified.OrderByDescending(l => ChanceToCash(l, points, threshold)).First();
                    var orderedLineup = best.OrderBy(p => p.Position).ThenBy(p => p.Name);
                    Console.WriteLine(string.Join(" ", new[]{
                        $"Chace to Cash: {ChanceToCash(best,points,threshold)}",
                        $"Salary: ${best.Sum(p => p.Salary)}"
                    }));
                    Console.WriteLine($"\t{string.Join(",", orderedLineup.Skip(0).Take(3).Select(p => p.Name))}");
                    Console.WriteLine($"\t{string.Join(",", orderedLineup.Skip(3).Take(3).Select(p => p.Name))}");
                    Console.WriteLine($"\t{string.Join(",", orderedLineup.Skip(6).Take(3).Select(p => p.Name))}");
                }
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            producer.Join();
            foreach (var consumer in consumers)
                consumer.Join();

            var lineups = qualified.Distinct(new LineupEqualityComparer()).ToArray();

            Console.WriteLine($"{lineups.Count()} lineups at least {threshold} points");

            lineups = lineups.OrderByDescending(l => ChanceToCash(l, points, threshold)).ToArray();

            foreach (var lineup in lineups.Take(20))
            {
                var orderedLineup = lineup.OrderBy(p => p.Position).ThenBy(p => p.Name);

                Console.WriteLine(string.Join(" ", new[]{
                        $"Chace to Cash: {ChanceToCash(lineup,points,threshold):P}",
                        $"Salary: ${lineup.Sum(p => p.Salary)}"
                    }));
                Console.WriteLine($"\t{string.Join(",", orderedLineup.Skip(0).Take(3).Select(p => p.Name + " " + p.Position))}");
                Console.WriteLine($"\t{string.Join(",", orderedLineup.Skip(3).Take(3).Select(p => p.Name + " " + p.Position))}");
                Console.WriteLine($"\t{string.Join(",", orderedLineup.Skip(6).Take(3).Select(p => p.Name + " " + p.Position))}");
            }
        }

        private double ChanceToCash(DailyPlayer[] lineup, Dictionary<string, Normal> points, float threshold)
        {
            var mean = 0.0;
            var totalVariance = 0.0;
            foreach (var player in lineup)
            {
                mean += points[player.Id].Mean;
                totalVariance += points[player.Id].Variance;
            }
            return 1 - new Normal(mean, Math.Sqrt(totalVariance)).CumulativeDistribution(threshold);
        }
    }
}
