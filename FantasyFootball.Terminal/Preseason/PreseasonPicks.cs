using Bovada;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Terminal.Preseason
{
    class PreseasonPicks
    {
        private class Odds
        {
            public string Team;
            public double Line;
            public double Payoff;
            public double Vig;

            public double Portion => (1 - Vig) / Payoff;
        }

        public static void Do(BovadaClient bovadaClient)
        {
            var allOdds = new List<Odds>();

            var bovadaEvents = bovadaClient.NflPreseason();
            const double bovadaVig = 0.045;
            foreach (var bovadaEvent in bovadaEvents)
            {
                var gameLineMarkets = bovadaEvent.displayGroups.Single(dg => dg.description == "Game Lines").markets;
                var pointSpreads = gameLineMarkets.SingleOrDefault(m => m.description == "Point Spread");
                if (pointSpreads != null)
                {
                    foreach (var outcome in pointSpreads.outcomes)
                    {
                        allOdds.Add(new Odds
                        {
                            Team = outcome.description.Split(' ').Last(),
                            Line = double.Parse(outcome.price.handicap),
                            Payoff = double.Parse(outcome.price.@decimal),
                            Vig = bovadaVig
                        });
                    }
                }
                var moneyLines = gameLineMarkets.SingleOrDefault(m => m.description == "Moneyline");
                if (moneyLines != null)
                {
                    foreach (var outcome in moneyLines.outcomes)
                    {
                        allOdds.Add(new Odds
                        {
                            Team = outcome.description.Split(' ').Last(),
                            Line = 0,
                            Payoff = double.Parse(outcome.price.@decimal),
                            Vig = bovadaVig
                        });
                    }
                }
            }

            var x = allOdds.GroupBy(o => o.Team).Select(g => new
            {
                Team = g.Key,
                Avg = g.Sum(o => o.Portion * o.Line) / g.Sum(o => o.Portion)
            });

            File.Delete("picks.txt");
            foreach (var y in x.OrderBy(z => z.Avg))
            {
                var t = allOdds.Where(o => o.Team == y.Team).OrderBy(o => o.Vig);

                File.AppendAllText("picks.txt", y.Team + "\n");

                Console.WriteLine(string.Join(" ", new string[]{
                    $"{y.Team,10}",
                    y.Avg.ToString("+0.00;-0.00"),
                    string.Join(",", t.Select(o => o.Line.ToString("+0.0;-0.0"))),
                    string.Join(",", t.Select(o => o.Payoff.ToString("F3"))),
                    string.Join(",", t.Select(o => o.Portion.ToString("P")))
                }));
            }
        }

        private static double toNormalOdds(double american)
        {
            if (american >= 0) return (american / 100) + 1;
            else return Math.Abs(100 / american) + 1;
        }
    }
}
