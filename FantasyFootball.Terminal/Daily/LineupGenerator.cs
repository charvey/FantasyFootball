using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Terminal.Daily
{
    public static class LineupGenerator
    {
        public static IEnumerable<DailyPlayer[]> GenerateLineups(DailyPlayer[] players)
        {
            var playersByPosition = players.GroupBy(p => p.Position).ToDictionary(g => g.Key, g => g.ToArray());

            foreach (var rbs in RBPairs(playersByPosition["RB"]))
                foreach (var wrs in WRSets(playersByPosition["WR"]))
                    foreach (var te in playersByPosition["TE"])
                        foreach (var flex in FlexPlayers(playersByPosition, rbs, wrs, te).ToArray())
                            foreach (var qb in playersByPosition["QB"])
                                foreach (var def in playersByPosition["DEF"])
                                    yield return rbs.Concat(wrs).Concat(new[] { qb, def, te, flex }).ToArray();
        }


        private static IEnumerable<DailyPlayer> FlexPlayers(Dictionary<string, DailyPlayer[]> playersByPosition,
            DailyPlayer[] rbs, DailyPlayer[] wrs, DailyPlayer te)
        {
            var selected = new HashSet<string>(new[] { te.Id }.Concat(rbs.Select(p => p.Id)).Concat(wrs.Select(p => p.Id)));
            return new[] { "RB", "WR", "TE" }.SelectMany(p => playersByPosition[p]).Where(p => !selected.Contains(p.Id));
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