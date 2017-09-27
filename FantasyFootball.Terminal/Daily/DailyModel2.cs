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
    public class DailyModel2
    {
        private readonly string dataDirectory;
        private readonly SQLiteConnection connection;
        private readonly FantasyPros fantasyPros;

        public DailyModel2(SQLiteConnection connection,string dataDirectory)
        {
            this.connection = connection;
            this.dataDirectory = dataDirectory;
            this.fantasyPros = new FantasyPros(dataDirectory);
        }

		static Dictionary<float, double> Combine(IReadOnlyDictionary<float, double> A, IReadOnlyDictionary<float, double> B)
		{
			var outcome = new Dictionary<float, double>();
			foreach (var a in A)
			{
				foreach (var b in B)
				{
					var newKey = a.Key + b.Key;
					var newValue = a.Value * b.Value;
					if (!outcome.ContainsKey(newKey)) outcome[newKey] = 0;
					outcome[newKey] += newValue;
				}
			}
			return outcome;
		}

		static Dictionary<float, double> Multiply(Dictionary<float, double> X, float m)
		{
			var outcome = new Dictionary<float, double>();
			foreach (var x in X)
				outcome[x.Key * m] = x.Value;
			return outcome;
		}

        IReadOnlyDictionary<float, double> ExpectedPoints(SQLiteConnection connection, DailyPlayer player)
        {
            var row = fantasyPros.GetPlayerRow(player);

			if (row == null)
			{
				Console.WriteLine("Can't find " + player.Name);
				return new Dictionary<float, double> { { 0, 1 } };
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

        private static Dictionary<float,double> ParseForQB(HtmlNode row)
		{
			var tds = row.Elements("td").ToArray();
			var outcome = new Dictionary<float, double> { { 0, 1 } };
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[3].InnerText)), 0.04f));
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[4].InnerText)), 4f));
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[5].InnerText)), -1f));
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[7].InnerText)), 0.1f));
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[8].InnerText)), 6f));
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[9].InnerText)), -2f));
			return outcome;
		}

		private static Dictionary<float, double> ParseForWR(HtmlNode row)
		{
            var tds=row.Elements("td").ToArray();
            var outcome=new Dictionary<float,double>{{0,1}};
            outcome=Combine(outcome,Multiply(Estimate(float.Parse(tds[2].InnerText)),0.1f));
            outcome=Combine(outcome,Multiply(Estimate(float.Parse(tds[3].InnerText)),6.0f));
            outcome=Combine(outcome,Multiply(Estimate(float.Parse(tds[4].InnerText)),0.5f));
            outcome=Combine(outcome,Multiply(Estimate(float.Parse(tds[5].InnerText)),0.1f));
            outcome=Combine(outcome,Multiply(Estimate(float.Parse(tds[6].InnerText)),6.0f));
            outcome=Combine(outcome,Multiply(Estimate(float.Parse(tds[7].InnerText)),-2f));
			return outcome;
		}

		private static Dictionary<float, double> ParseForRB(HtmlNode row)
		{
			var tds = row.Elements("td").ToArray();
			var outcome = new Dictionary<float, double> { { 0, 1 } };
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[2].InnerText)), 0.1f));
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[3].InnerText)), 6.0f));
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[4].InnerText)), 0.5f));
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[5].InnerText)), 0.1f));
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[6].InnerText)), 6.0f));
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[7].InnerText)), -2f));
			return outcome;
		}

		private static Dictionary<float, double> ParseForTE(HtmlNode row)
		{
			var tds = row.Elements("td").ToArray();
			var outcome = new Dictionary<float, double> { { 0, 1 } };
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[1].InnerText)), 0.5f));
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[2].InnerText)), 0.1f));
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[3].InnerText)), 6.0f));
			outcome = Combine(outcome, Multiply(Estimate(float.Parse(tds[4].InnerText)), -2f));
			return outcome;
		}

		private static Dictionary<float, double> ParseForDST(HtmlNode row)
		{
			return Estimate(float.Parse(row.Elements("td").ToArray()[10].InnerText));
		}

		private static Dictionary<float, double> Estimate(float mean)
		{
            var normal = new Normal(mean, Math.Sqrt(mean));

			var count = 8.0;
			var a = Enumerable.Range(1,(int) count);
			var b = a.Select(p => ((double)p) / 10.0);
			var c = b.Select(p => new { p = 1.0/count, x = Math.Round(Math.Max(0, normal.InverseCumulativeDistribution(p))) });
			var d = c.GroupBy(_ => (float)(int)_.x);
			var e = d.ToDictionary(g => g.Key, g => g.Sum(_ => _.p));
			return e;
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
			players = players.Where(p => !points[p.Id].ContainsKey(0f) || points[p.Id][0f] < 1).ToArray();
            Console.WriteLine($"{sw.Elapsed} {players.Length} players expected to get any points");
            players = players.Where(player => points[player.Id].Where(p => p.Key > 5).Sum(p => p.Value) > 0.75).ToArray();
            Console.WriteLine($"{sw.Elapsed} {players.Length} players with 75% chance to get 5 points");
            // players = players.Where(player =>
            // {
            //     return !players
            //     .Where(p => p.Position == player.Position)
            //     .Where(p => p.Salary <= player.Salary)
            //     .Where(p => points[p.Id] > points[player.Id])
            //     .Any();
            // }).ToArray();
            // Console.WriteLine($"{sw.Elapsed} {players.Length} players who are strictly best with regard to salary");

            //var average = players.Average(p => points[p.Id]);
			var threshold = 100f;//average * 9 * (10.0 / 9);
								//Console.WriteLine($"{sw.Elapsed} Average score of players: {average} Threshold: {threshold}");

			

			var done = false;
			var queue = new ConcurrentQueue<DailyPlayer[]>();
			var qualified = new ConcurrentBag<DailyPlayer[]>();
			var processed = 0L;

			var producer = new Thread(() =>
			  {
				  foreach (var lineup in LineupGenerator.GenerateLineups(players, budget).Where(l => l.Sum(p => p.Salary) <= budget))
				  {
					  queue.Enqueue(lineup);
					  if (queue.Count > 100000) Thread.Sleep(TimeSpan.FromSeconds(1));
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
                            if (ChanceToCash(lineup, points, threshold) > 0.666f)
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
                var orderedLineup = lineup.OrderBy(p => p.Position).ThenBy(p => points[p.Id]).ThenBy(p => p.Name);

                Console.WriteLine(string.Join(" ", new[]{
                        $"Chace to Cash: {ChanceToCash(lineup,points,threshold):P}",
                        $"Salary: ${lineup.Sum(p => p.Salary)}"
                    }));
                Console.WriteLine($"\t{string.Join(",", orderedLineup.Skip(0).Take(3).Select(p => p.Name))}");
                Console.WriteLine($"\t{string.Join(",", orderedLineup.Skip(3).Take(3).Select(p => p.Name))}");
                Console.WriteLine($"\t{string.Join(",", orderedLineup.Skip(6).Take(3).Select(p => p.Name))}");
            }
        }

        private ConcurrentDictionary<DailyPlayer[], double> chances = new ConcurrentDictionary<DailyPlayer[], double>();
        private double ChanceToCash(DailyPlayer[] lineup, Dictionary<string, IReadOnlyDictionary<float, double>> points, float threshold)
		{
            return chances.GetOrAdd(lineup, l =>
            {
                var outcome = LineupDistribution(string.Join(",", lineup.Select(p => p.Id).Reverse()), points);
                return outcome.Where(x => x.Key >= threshold).Sum(x => x.Value);
            });
        }

        private ConcurrentDictionary<string, IReadOnlyDictionary<float, double>> lineupDist = new ConcurrentDictionary<string, IReadOnlyDictionary<float, double>>();
        private IReadOnlyDictionary<float, double> LineupDistribution(string lineupKey, Dictionary<string, IReadOnlyDictionary<float, double>> points)
        {
            return lineupDist.GetOrAdd(lineupKey, k =>
            {
                if (k == "") return new Dictionary<float, double> { { 0, 1 } };

                var parts = k.Split(new[] { ',' }, 2);

                if (parts.Length == 1)
                    return points[parts[0]];

                return Combine(LineupDistribution(parts[0], points), LineupDistribution(parts[1], points));
            });
        }
    }
}
