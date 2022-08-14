using FantasyPros;
using FantasyPros.Projections;
using MathNet.Numerics.Distributions;
using System.Collections.Concurrent;
using System.Data.SQLite;
using System.Diagnostics;
using YahooDailyFantasy;

namespace FantasyFootball.Terminal.Daily
{
    public class DailyModel2
    {
        private readonly SQLiteConnection connection;
        private readonly TextWriter output;
        private readonly FantasyProsClient fantasyPros;

        public DailyModel2(SQLiteConnection connection, TextWriter output, FantasyProsClient fantasyProsClient)
        {
            this.connection = connection;
            this.output = output;
            this.fantasyPros = fantasyProsClient;
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

        IReadOnlyDictionary<float, double> ExpectedPoints(DailyPlayer player, DateTime at)
        {
            var playerId = fantasyPros.TempGetPlayerId(player.Name);

            if (playerId == null)
            {
                output.WriteLine($"Can't find {player.Name} {player.Salary:C}");
                return new Dictionary<float, double> { { 0, 1 } };
            }

            switch (player.Position)
            {
                case "QB": return EstimateForQB(fantasyPros.GetQbProjection(playerId, at));
                case "WR": return EstimateForWr(fantasyPros.GetWrProjection(playerId, at));
                case "RB": return EstimateForRB(fantasyPros.GetRbProjection(playerId, at));
                case "TE": return EstimateForTE(fantasyPros.GetTeProjection(playerId, at));
                case "DEF": return EstimateForDST(fantasyPros.GetDstProjection(playerId, at));
                default: throw new ArgumentOutOfRangeException();
            }
        }

        private static IReadOnlyDictionary<float, double> EstimateForQB(QbProjection projection)
        {
            var outcome = new Dictionary<float, double> { { 0, 1 } };
            outcome = Combine(outcome, Multiply(Estimate(projection.PassingYards), 0.04f));
            outcome = Combine(outcome, Multiply(Estimate(projection.PassingTouchdowns), 4f));
            outcome = Combine(outcome, Multiply(Estimate(projection.Interceptions), -1f));
            outcome = Combine(outcome, Multiply(Estimate(projection.RushingYards), 0.1f));
            outcome = Combine(outcome, Multiply(Estimate(projection.RushingTouchdowns), 6f));
            outcome = Combine(outcome, Multiply(Estimate(projection.Fumbles), -2f));
            return outcome;
        }

        private static IReadOnlyDictionary<float, double> EstimateForWr(WrProjection projection)
        {
            var outcome = new Dictionary<float, double> { { 0, 1 } };
            outcome = Combine(outcome, Multiply(Estimate(projection.ReceivingYards), 0.1f));
            outcome = Combine(outcome, Multiply(Estimate(projection.ReceivingTouchdowns), 6.0f));
            outcome = Combine(outcome, Multiply(Estimate(projection.Receptions), 0.5f));
            outcome = Combine(outcome, Multiply(Estimate(projection.RushingYards), 0.1f));
            outcome = Combine(outcome, Multiply(Estimate(projection.RushingTouchdowns), 6.0f));
            outcome = Combine(outcome, Multiply(Estimate(projection.Fumbles), -2f));
            return outcome;
        }

        private static IReadOnlyDictionary<float, double> EstimateForRB(RbProjection projection)
        {
            var outcome = new Dictionary<float, double> { { 0, 1 } };
            outcome = Combine(outcome, Multiply(Estimate(projection.ReceivingYards), 0.1f));
            outcome = Combine(outcome, Multiply(Estimate(projection.ReceivingTouchdowns), 6.0f));
            outcome = Combine(outcome, Multiply(Estimate(projection.Receptions), 0.5f));
            outcome = Combine(outcome, Multiply(Estimate(projection.RushingYards), 0.1f));
            outcome = Combine(outcome, Multiply(Estimate(projection.RushingTouchdowns), 6.0f));
            outcome = Combine(outcome, Multiply(Estimate(projection.Fumbles), -2f));
            return outcome;
        }

        private static IReadOnlyDictionary<float, double> EstimateForTE(TeProjection projection)
        {
            var outcome = new Dictionary<float, double> { { 0, 1 } };
            outcome = Combine(outcome, Multiply(Estimate(projection.Receptions), 0.5f));
            outcome = Combine(outcome, Multiply(Estimate(projection.ReceivingYards), 0.1f));
            outcome = Combine(outcome, Multiply(Estimate(projection.ReceivingTouchdowns), 6.0f));
            outcome = Combine(outcome, Multiply(Estimate(projection.Fumbles), -2f));
            return outcome;
        }

        private static IReadOnlyDictionary<float, double> EstimateForDST(DstProjection projection) => Estimate(projection.FantasyPoints);

        private static Dictionary<float, double> Estimate(float mean)
        {
            var normal = new Normal(mean, Math.Sqrt(mean));

            var count = 8.0;
            var a = Enumerable.Range(1, (int)count);
            var b = a.Select(p => ((double)p) / 10.0);
            var c = b.Select(p => new { p = 1.0 / count, x = Math.Round(Math.Max(0, normal.InverseCumulativeDistribution(p))) });
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

        public void Do(YahooDailyFantasyClient yahooDailyFantasyClient, int contestId)
        {
            var contest = yahooDailyFantasyClient.GetContest(contestId);
            var budget = contest.salaryCap;
            var startTime = new DateTime(1970, 1, 1).AddMilliseconds(contest.startTime);

            var sw = Stopwatch.StartNew();

            var players = yahooDailyFantasyClient.GetPlayers(contestId).Select(ydp => new DailyPlayer
            {
                Id = ydp.Id,
                Name = $"{ydp.FirstName} {ydp.LastName}",
                Position = ydp.Position,
                Salary = ydp.Salary
            }).ToArray();
            var playerLookup = players.ToDictionary(p => p.Id);

            output.WriteLine($"{sw.Elapsed} {players.Length} players eligible");
            var points = players.ToDictionary(p => p.Id, p => ExpectedPoints(p, startTime));
            players = players.Where(p => !points[p.Id].ContainsKey(0f) || points[p.Id][0f] < 1).ToArray();
            output.WriteLine($"{sw.Elapsed} {players.Length} players expected to get any points");
            players = players.Where(player => points[player.Id].Where(p => p.Key > 5).Sum(p => p.Value) > 0.75).ToArray();
            output.WriteLine($"{sw.Elapsed} {players.Length} players with 75% chance to get 5 points");

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
                output.WriteLine($"{sw.Elapsed} {queue.Count} {qualified.Count}/{processed}");
                if (qualified.Any())
                {
                    var best = qualified.OrderByDescending(l => ChanceToCash(l, points, threshold)).First();
                    var orderedLineup = best.OrderBy(p => p.Position).ThenBy(p => p.Name);
                    output.WriteLine(string.Join(" ", new[]{
                        $"Chace to Cash: {ChanceToCash(best,points,threshold)}",
                        $"Salary: ${best.Sum(p => p.Salary)}"
                    }));
                    output.WriteLine($"\t{string.Join(",", orderedLineup.Skip(0).Take(3).Select(p => p.Name))}");
                    output.WriteLine($"\t{string.Join(",", orderedLineup.Skip(3).Take(3).Select(p => p.Name))}");
                    output.WriteLine($"\t{string.Join(",", orderedLineup.Skip(6).Take(3).Select(p => p.Name))}");
                }
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            producer.Join();
            foreach (var consumer in consumers)
                consumer.Join();

            var lineups = qualified.Distinct(new LineupEqualityComparer()).ToArray();

            output.WriteLine($"{lineups.Count()} lineups at least {threshold} points");

            lineups = lineups.OrderByDescending(l => ChanceToCash(l, points, threshold)).ToArray();

            foreach (var lineup in lineups.Take(20))
            {
                var orderedLineup = lineup.OrderBy(p => p.Position).ThenBy(p => points[p.Id]).ThenBy(p => p.Name);

                output.WriteLine(string.Join(" ", new[]{
                        $"Chace to Cash: {ChanceToCash(lineup,points,threshold):P}",
                        $"Salary: ${lineup.Sum(p => p.Salary)}"
                    }));
                output.WriteLine($"\t{string.Join(",", orderedLineup.Skip(0).Take(3).Select(p => p.Name))}");
                output.WriteLine($"\t{string.Join(",", orderedLineup.Skip(3).Take(3).Select(p => p.Name))}");
                output.WriteLine($"\t{string.Join(",", orderedLineup.Skip(6).Take(3).Select(p => p.Name))}");
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
