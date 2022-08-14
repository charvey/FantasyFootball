using FantasyPros;
using FantasyPros.Projections;
using MathNet.Numerics.Distributions;
using System.Collections.Concurrent;
using System.Data.SQLite;
using System.Diagnostics;
using YahooDailyFantasy;

namespace FantasyFootball.Terminal.Daily
{
    public class DailyModel4
    {
        private readonly TextWriter output;
        private readonly SQLiteConnection connection;
        private readonly FantasyProsClient fantasyPros;

        private Dictionary<string, Normal> points;
        private float threshold;

        public DailyModel4(SQLiteConnection connection, TextWriter output, FantasyProsClient fantasyProsClient)
        {
            this.connection = connection;
            this.output = output;
            this.fantasyPros = fantasyProsClient;
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
            var playerId = fantasyPros.TempGetPlayerId(player.Name);

            if (playerId == null)
            {
                output.WriteLine($"Can't find {player.Name} {player.Salary:C}");
                return new Normal(0, 0);
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

        private static Normal EstimateForQB(QbProjection projection)
        {
            return new Normal(projection.FantasyPoints + -0.248648649, 8.190060608);
        }

        private static Normal EstimateForWr(WrProjection projection)
        {
            return new Normal(projection.FantasyPoints + 0.473846154, 5.445951974);
        }

        private static Normal EstimateForRB(RbProjection projection)
        {
            return new Normal(projection.FantasyPoints + 0.639215686, 5.866557879);

        }

        private static Normal EstimateForTE(TeProjection projection)
        {
            return new Normal(projection.FantasyPoints + -0.521428571, 4.170368016);
        }

        private static Normal EstimateForDST(DstProjection projection) => new Normal(projection.FantasyPoints + 1.39375, 5.714069123);

        private static Normal Estimate(float mean)
        {
            return new Normal(mean, Math.Sqrt(Math.Abs(mean)));
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
            output.WriteLine($"{sw.Elapsed} {players.Length} players eligible ({string.Join(",", players.GroupBy(p => p.Position).Select(g => g.Key + ":" + g.Count()))})");
            points = players.ToDictionary(p => p.Id, p => ExpectedPoints(p, startTime));
            threshold = 100f;

            var baseLineByPosition = new Dictionary<string, double>
            {
                {"QB",1 },
                {"DEF",1 },
                {"RB",2 },
                {"WR",2 },
                {"TE",2 }
            }.ToDictionary(x => x.Key, x => x.Value * 1);

            output.WriteLine($"Filtering by targets of {string.Join(",", baseLineByPosition.Select(x => x.Key + ":" + x.Value))}");
            players = players.Where(player => points[player.Id].InverseCumulativeDistribution(.4125) >= baseLineByPosition[player.Position]).ToArray();
            output.WriteLine($"{sw.Elapsed} {players.Length} players who are above targets ({string.Join(",", players.GroupBy(p => p.Position).Select(g => g.Key + ":" + g.Count()))})");

            var queue = new BlockingCollectionSlim<DailyPlayer[]>(20_000_000);
            var qualified = new ConcurrentDictionary<DailyPlayer[], double>();
            var processed = 0L;
            var qualifiedCount = 0L;
            var producer = new LinqLineupGeneratorProducer(players, budget);

            new Thread(() => producer.Start(queue)).Start();

            var consumers = new Thread[Environment.ProcessorCount * 3 / 4];
            for (var i = 0; i < consumers.Length; i++)
            {
                consumers[i] = new Thread(() =>
                {
                    while (!queue.IsCompleted)
                    {
                        DailyPlayer[] lineup;
                        if (queue.TryTake(out lineup, TimeSpan.FromSeconds(1)))
                        {
                            var chanceToCash = ChanceToCash(lineup);
                            if (chanceToCash > 0.6f)
                            {
                                if (!qualified.TryAdd(lineup, chanceToCash))
                                    throw new InvalidOperationException();
                                Interlocked.Increment(ref qualifiedCount);
                            }
                            else
                            {
                                producer.Release(lineup);
                            }
                            Interlocked.Increment(ref processed);
                        }
                    }
                });
            }

            foreach (var consumer in consumers)
                consumer.Start();
            while (!queue.IsCompleted)
            {
                output.WriteLine($"{sw.Elapsed} {queue.Count} {qualifiedCount}/{processed} {processed}/{producer.Total} ({1.0 * processed / producer.Total:P})");
                if (qualified.Any())
                {
                    var ordered = qualified.ToArray().OrderByDescending(l => l.Value);
                    var best = ordered.First();
                    DisplayInfo(best.Key);
                    foreach (var x in ordered.Skip(1000))
                    {
                        if (!qualified.TryRemove(x.Key, out double _))
                            throw new Exception();
                        producer.Release(x.Key);
                    }
                }
                Thread.Sleep(TimeSpan.FromSeconds(5));
            }
            foreach (var consumer in consumers)
                consumer.Join();

            var lineups = qualified.Keys.Distinct(new LineupEqualityComparer()).ToArray();

            output.WriteLine();
            output.WriteLine($"{lineups.Count()} lineups at least {threshold} points");

            lineups = lineups.OrderByDescending(l => ChanceToCash(l)).ToArray();

            var rank = 1;
            foreach (var lineup in lineups.Take(20))
            {
                output.WriteLine($"\n#{rank}");
                DisplayInfo(lineup);
                rank++;
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
