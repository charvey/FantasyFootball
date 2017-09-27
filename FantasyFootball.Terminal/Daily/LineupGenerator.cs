using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Terminal.Daily
{
    public static class LineupGenerator
    {
        public static IEnumerable<DailyPlayer[]> GenerateLineups(DailyPlayer[] players, int budget)
        {
            var playersByPosition = players.GroupBy(p => p.Position)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.Salary).ToArray());

            var rbPairs = RBPairs(playersByPosition["RB"]).ToArray();
            var wrSets = WRSets(playersByPosition["WR"]).ToArray();

            Console.WriteLine(1L * rbPairs.Length * wrSets.Length
                * playersByPosition["TE"].Length * playersByPosition["QB"].Length * playersByPosition["DEF"].Length
                * (playersByPosition["RB"].Length + playersByPosition["WR"].Length + playersByPosition["TE"].Length - 5));

            foreach (var rbs in rbPairs)
            {
                var budgetUpToRbs = rbs.Sum(p => p.Salary);
                if (budgetUpToRbs > budget) continue;

                foreach (var wrs in wrSets)
                {
                    var budgetUpToWrs = budgetUpToRbs + wrs.Sum(p => p.Salary);
                    if (budgetUpToWrs > budget) continue;

                    foreach (var te in playersByPosition["TE"])
                    {
                        var budgetUpToTe = budgetUpToWrs + te.Salary;
                        if (budgetUpToTe > budget) continue;

                        foreach (var qb in playersByPosition["QB"])
                        {
                            var budgetUpToQb = budgetUpToTe + qb.Salary;
                            if (budgetUpToQb > budget) continue;

                            foreach (var def in playersByPosition["DEF"])
                            {
                                var budgetUpToDef = budgetUpToQb + def.Salary;
                                if (budgetUpToDef > budget) continue;

                                foreach (var flex in FlexPlayers(playersByPosition, rbs, wrs, te, budget - budgetUpToDef).ToArray())
                                {
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