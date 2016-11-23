using FantasyFootball.Terminal.GameStateModels;
using FantasyFootball.Terminal.Providers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Terminal.Modelers
{
    public class MatchupResolver
    {
        private Random random;
        private PredictionProvider predictionProvider;

        public MatchupResolver(PredictionProvider predictionProvider)
        {
            this.predictionProvider = predictionProvider;
        }

        public double NextNormal()
        {
            double u1 = random.NextDouble();
            double u2 = random.NextDouble();
            return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Sin(2.0 * Math.PI * u2);
        }

        public double Normal(double mu, double stddev)
        {
            return mu + NextNormal() * stddev;
        }

        public Team ResolveWinner(GameState state, Matchup matchup)
        {
            var estimateA = EstimateScore(state, matchup.Week, matchup.TeamA);
            var estimateB = EstimateScore(state, matchup.Week, matchup.TeamB);

            var actualA = Normal(estimateA, 10);
            var actualB = Normal(estimateB, 10);

            if (actualA > actualB)
                return matchup.TeamA;
            else
                return matchup.TeamB;
        }

        public double EstimateScore(GameState state, int week, Team team)
        {
            return PickRoster(state.Roster(team), week).Sum(p => predictionProvider.Get(p, week));
        }

        public IEnumerable<Player> PickRoster(IEnumerable<Player> players, int week)
        {
            players = players.OrderByDescending(p => predictionProvider.Get(p, week));

            var spots = new[]
            {
                new[] {"QB"},
                new[] {"WR"},
                new[] {"WR"},
                new[] {"RB"},
                new[] {"RB"},
                new[] {"TE"},
                new[] {"WR","RB"},
                new[] {"WR","RB","TE"},
                new[] {"K"},
                new[] {"DEF"}
            };

            foreach (var spot in spots)
            {
                var player = players.FirstOrDefault(p => spot.Any(rp => p.Positions.Contains(rp)));
                if (player != null)
                {
                    yield return player;
                    players = players.Where(p => p != player);
                }
            }
        }
    }
}
