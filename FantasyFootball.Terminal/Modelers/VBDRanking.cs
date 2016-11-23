using FantasyFootball.Terminal.GameStateModels;
using FantasyFootball.Terminal.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyFootball.Terminal.Modelers
{
    public class VBDRanking
    {
        private PredictionProvider predictions;

        public VBDRanking(PredictionProvider predictions)
        {
            this.predictions = predictions;
        }

        private IEnumerable<Player> OrderByVBD(IEnumerable<Player> players)
        {
            var pointTotals = players.ToDictionary(p => p, p => Enumerable.Range(1, 17).Select(w => predictions.Get(p, w)).Sum());
            var qbBaseline = players.Where(p => p.Positions.Contains("QB")).Select(p => pointTotals[p]).OrderByDescending(p => p).ElementAt(12 * 1 - 1);
            var rbBaseline = players.Where(p => p.Positions.Contains("RB")).Select(p => pointTotals[p]).OrderByDescending(p => p).ElementAt(12 * 2 - 1);
            var wrbBaseline = players.Where(p => p.Positions.Contains("WR")).Select(p => pointTotals[p]).OrderByDescending(p => p).ElementAt(12 * 2 - 1);
            var teBaseline = players.Where(p => p.Positions.Contains("TE")).Select(p => pointTotals[p]).OrderByDescending(p => p).ElementAt(12 * 1 - 1);
            var kBaseline = players.Where(p => p.Positions.Contains("K")).Select(p => pointTotals[p]).OrderByDescending(p => p).ElementAt(12 * 1 - 1);
            var defbBaseline = players.Where(p => p.Positions.Contains("DEF")).Select(p => pointTotals[p]).OrderByDescending(p => p).ElementAt(12 * 1 - 1);

            return players.OrderByDescending(p =>
            {
                return pointTotals[p] - p.Positions.Select(pos =>
                {
                    switch (pos)
                    {
                        case "QB":
                            return qbBaseline;
                        case "RB":
                            return rbBaseline;
                        case "WR":
                            return wrbBaseline;
                        case "TE":
                            return teBaseline;
                        case "K":
                            return kBaseline;
                        case "DEF":
                            return defbBaseline;
                        default:
                            throw new Exception();
                    }
                }).Min();
            });
        }
    }
}
