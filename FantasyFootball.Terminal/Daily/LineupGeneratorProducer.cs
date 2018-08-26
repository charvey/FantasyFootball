using FantasyPros;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace FantasyFootball.Terminal.Daily
{
    public class LineupGeneratorProducer
    {
        private readonly DailyPlayer[] players;
        private readonly int budget;

        public LineupGeneratorProducer(DailyPlayer[] players, int budget)
        {
            this.players = players;
            this.budget = budget;
        }

        public long Total;
        public long Skipped;
        public long Returned;
        public long Done => Skipped + Returned;


        public void Start(ConcurrentQueue<DailyPlayer[]> output, int buffer = 1000000)
        {
            foreach(var lineup in Generate())
            {
                while (output.Count > buffer) Thread.Yield();
                output.Enqueue(lineup);
            }
        }

        private IEnumerable<DailyPlayer[]> Generate()
        {
            var playersByPosition = players.GroupBy(p => p.Position)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.Salary).ToArray());

            var rbPairs = RBPairs(playersByPosition["RB"]).ToArray();
            var wrSets = WRSets(playersByPosition["WR"]).ToArray();

            foreach(var x in playersByPosition)
            {
                Console.WriteLine($"{x.Key}:{x.Value.Length}");
            }

            Console.WriteLine(1L * rbPairs.Length * wrSets.Length
                * playersByPosition["TE"].Length
                * (playersByPosition["RB"].Length + playersByPosition["WR"].Length + playersByPosition["TE"].Length - 5) + " Offensive Combinations");

            Total = 1L * rbPairs.Length * wrSets.Length
                * playersByPosition["TE"].Length * playersByPosition["QB"].Length * playersByPosition["DEF"].Length
                * (playersByPosition["RB"].Length + playersByPosition["WR"].Length + playersByPosition["TE"].Length - 5);
            Skipped = 0;
            Returned = 0;

            foreach (var rbs in rbPairs)
            {
                var budgetUpToRbs = rbs.Sum(p => p.Salary);
                if (budgetUpToRbs > budget)
                {
                    Skipped += 1L
                        * wrSets.Length * playersByPosition["TE"].Length
                        * (playersByPosition["RB"].Length + playersByPosition["WR"].Length + playersByPosition["TE"].Length - 5)
                        * playersByPosition["QB"].Length * playersByPosition["DEF"].Length;
                    continue;
                }

                foreach (var wrs in wrSets)
                {
                    var budgetUpToWrs = budgetUpToRbs + wrs.Sum(p => p.Salary);
                    if (budgetUpToWrs > budget)
                    {
                        Skipped += 11L
                            * playersByPosition["TE"].Length
                            * (playersByPosition["RB"].Length + playersByPosition["WR"].Length + playersByPosition["TE"].Length - 5)
                            * playersByPosition["QB"].Length * playersByPosition["DEF"].Length;
                        continue;
                    }

                    foreach (var te in playersByPosition["TE"])
                    {
                        var budgetUpToTe = budgetUpToWrs + te.Salary;
                        if (budgetUpToTe > budget)
                        {
                            Skipped += 1L
                                * (playersByPosition["RB"].Length + playersByPosition["WR"].Length + playersByPosition["TE"].Length - 5)
                                * playersByPosition["QB"].Length * playersByPosition["DEF"].Length;
                            continue;
                        }

                        foreach (var qb in playersByPosition["QB"])
                        {
                            var budgetUpToQb = budgetUpToTe + qb.Salary;
                            if (budgetUpToQb > budget)
                            {
                                Skipped += 1L
                                    * (playersByPosition["RB"].Length + playersByPosition["WR"].Length + playersByPosition["TE"].Length - 5)
                                     * playersByPosition["DEF"].Length;
                                continue;
                            }

                            foreach (var def in playersByPosition["DEF"])
                            {
                                var budgetUpToDef = budgetUpToQb + def.Salary;
                                if (budgetUpToDef > budget)
                                {
                                    Skipped += 1L
                                        * (playersByPosition["RB"].Length + playersByPosition["WR"].Length + playersByPosition["TE"].Length - 5);
                                    continue;
                                }

                                foreach (var flex in FlexPlayers(playersByPosition, rbs, wrs, te, budget - budgetUpToDef).ToArray())
                                {
                                    Returned += 1L;
                                    yield return rbs.Concat(wrs).Concat(new[] { qb, def, te, flex }).ToArray();
                                }
                            }
                        }
                    }
                }
            }
        }

        private static IEnumerable<DailyPlayer> FlexPlayers(Dictionary<string, DailyPlayer[]> playersByPosition,
            DailyPlayer[] rbs, DailyPlayer[] wrs, DailyPlayer te,
            int remainingBudget)
        {
            var selected = new HashSet<string>(new[] { te.Id }.Concat(rbs.Select(p => p.Id)).Concat(wrs.Select(p => p.Id)));
            return new[] { "RB", "WR", "TE" }.SelectMany(p => playersByPosition[p])
                .Where(p => p.Salary <= remainingBudget)
                .Where(p => !selected.Contains(p.Id));
        }

        private static IEnumerable<DailyPlayer[]> RBPairs(DailyPlayer[] rbs)
        {
            for (int i = 0; i < rbs.Length; i++)
                for (int j = i + 1; j < rbs.Length; j++)
                    yield return new[] { rbs[i], rbs[j] };
        }

        private static IEnumerable<DailyPlayer[]> WRSets(DailyPlayer[] wrs)
        {
            for (int i = 0; i < wrs.Length; i++)
                for (int j = i + 1; j < wrs.Length; j++)
                    for (int k = j + 1; k < wrs.Length; k++)
                        yield return new[] { wrs[i], wrs[j], wrs[k] };
        }
    }
}