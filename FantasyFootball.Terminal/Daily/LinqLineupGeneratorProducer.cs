using FantasyPros;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Terminal.Daily
{
    public class LinqLineupGeneratorProducer
    {
        private readonly DailyPlayer[] players;
        private readonly int budget;
        private readonly BlockingCollectionSlim<DailyPlayer[]> pool;

        public LinqLineupGeneratorProducer(DailyPlayer[] players, int budget)
        {
            this.players = players;
            this.budget = budget;
            this.pool = new BlockingCollectionSlim<DailyPlayer[]>();
        }

        public long Total;

        public void Start(BlockingCollectionSlim<DailyPlayer[]> output)
        {
            Enumerable.Repeat(0, output.Capacity).Select(_ => new DailyPlayer[9]).ToList().ForEach(l => pool.Add(l));
            Generate().ForAll(output.Add);
            output.CompleteAdding();
        }

        public void Release(DailyPlayer[] l) => pool.Add(l);

        private ParallelQuery<DailyPlayer[]> Generate()
        {
            var playersByPosition = players.GroupBy(p => p.Position)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.Salary).ToArray());

            var teSingles = TESingles(playersByPosition["TE"]).OrderByDescending(s => s.TotalSalary).ToArray();
            var rbPairs = RBPairs(playersByPosition["RB"]).OrderByDescending(s => s.TotalSalary).ToArray();
            var wrSets = WRSets(playersByPosition["WR"]).OrderByDescending(s => s.TotalSalary).ToArray();

            foreach (var x in playersByPosition)
            {
                Console.WriteLine($"{x.Key}:{x.Value.Length}");
            }

            Console.WriteLine(1L * rbPairs.Length * wrSets.Length * teSingles.Length + " Offensive Combinations");

            Console.WriteLine(1L * rbPairs.Length * wrSets.Length * teSingles.Length
                * (playersByPosition["RB"].Length + playersByPosition["WR"].Length + playersByPosition["TE"].Length - 5) + " Offensive Combinations with Flex");

            Total = 1L * rbPairs.Length * wrSets.Length * teSingles.Length
                * playersByPosition["QB"].Length * playersByPosition["DEF"].Length
                * (playersByPosition["RB"].Length + playersByPosition["WR"].Length + playersByPosition["TE"].Length - 5);

            var specialties = playersByPosition["QB"].SelectMany(q =>
                playersByPosition["DEF"].Select(d => new Subset
                (
                    players: new[] { q, d },
                    remainder: new DailyPlayer[0],
                    totalSalary: q.Salary + d.Salary
                ))
            ).OrderByDescending(s => s.TotalSalary).ToArray();

            return rbPairs.AsParallel()
                .SelectMany(r =>
                    wrSets.SelectMany(w =>
                        teSingles.SkipWhile(t => t.TotalSalary > (budget - r.TotalSalary - w.TotalSalary)).SelectMany(t =>
                            r.Remainder.Concat(w.Remainder).Concat(t.Remainder).SelectMany(f =>
                                specialties.SkipWhile(s => s.TotalSalary > (budget - r.TotalSalary - w.TotalSalary - t.TotalSalary - f.Salary)).Select(s =>
                                    {
                                        var l = pool.Take();
                                        l[0] = r.Players[0];
                                        l[1] = r.Players[1];
                                        l[2] = w.Players[0];
                                        l[3] = w.Players[1];
                                        l[4] = w.Players[2];
                                        l[5] = t.Players[0];
                                        l[6] = f;
                                        l[7] = s.Players[0];
                                        l[8] = s.Players[1];
                                        return l;
                                    }
                                )
                            )
                        )
                    )
                );
        }

        private static IEnumerable<Subset> TESingles(DailyPlayer[] tes)
        {
            for (int i = 0; i < tes.Length; i++)
                yield return new Subset
                (
                    players: new[] { tes[i] },
                    remainder: tes.Skip(i + 1).ToArray(),
                    totalSalary: tes[i].Salary
                );
        }

        private static IEnumerable<Subset> RBPairs(DailyPlayer[] rbs)
        {
            for (int i = 0; i < rbs.Length; i++)
                for (int j = i + 1; j < rbs.Length; j++)
                    yield return new Subset
                    (
                        players: new[] { rbs[i], rbs[j] },
                        remainder: rbs.Skip(j + 1).ToArray(),
                        totalSalary: rbs[i].Salary + rbs[j].Salary
                    );
        }

        private static IEnumerable<Subset> WRSets(DailyPlayer[] wrs)
        {
            for (int i = 0; i < wrs.Length; i++)
                for (int j = i + 1; j < wrs.Length; j++)
                    for (int k = j + 1; k < wrs.Length; k++)
                        yield return new Subset
                        (
                            players: new[] { wrs[i], wrs[j], wrs[k] },
                            remainder: wrs.Skip(k + 1).ToArray(),
                            totalSalary: wrs[i].Salary + wrs[j].Salary + wrs[k].Salary
                        );
        }

        private class Subset
        {
            public readonly DailyPlayer[] Players;
            public readonly DailyPlayer[] Remainder;
            public readonly int TotalSalary;

            public Subset(DailyPlayer[] players, DailyPlayer[] remainder, int totalSalary)
            {
                this.Players = players;
                this.Remainder = remainder;
                this.TotalSalary = totalSalary;
            }
        }
    }
}