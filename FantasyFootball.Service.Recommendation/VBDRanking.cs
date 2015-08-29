using FantasyFootball.Service.Fantasy;
using FantasyFootball.Service.Fantasy.Models;
using FantasyFootball.Service.Football;
using FantasyFootball.Service.Football.Models;
using FantasyFootball.Service.Prediction;
using Microsoft.Data.Entity;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Service.Recommendation
{
    // This project can output the Class library as a NuGet Package.
    // To enable this option, right-click on the project and select the Properties menu item. In the Build tab select "Produce outputs on build".
    public class VBDRanking
    {
        private readonly FantasyContext fantasyContext;
        private readonly FootballContext footballContext;
        private readonly PredictionContext predictionContext;

        public VBDRanking(FantasyContext fantasy,FootballContext football,PredictionContext prediction)
        {
            this.fantasyContext = fantasy;
            this.footballContext = football;
            this.predictionContext = prediction;
        }

        private IEnumerable<LeaguePlayer> LeaguePlayers(string leagueId)
        {
            return fantasyContext.Leagues
                .Include(l => l.Players)
                .Include(l => l.RosterPositions)
                .Single(l => l.Id == leagueId)
                .Players;
        }

        private Dictionary<LeaguePlayer, double> PlayerTotals(IEnumerable<LeaguePlayer> players, Guid runId)
        {
            return players.ToDictionary(player => player, player =>
            {
                var predictions = predictionContext.Predictions
                    .Include(r => r.Run)
                    .Where(p => p.PlayerId == player.PlayerId)
                    .Where(p => p.Run.Id == runId)
                    .ToList();

                var total = Enumerable.Range(1, 17)
                    .Select(w => predictions.Single(p => p.Week == w))
                    .Sum(p => p.Value);

                return total;
            });
        }

        public IEnumerable<Tuple<double, Player>> Get(string leagueId, Guid runId)
        {
            var playerRawValues = PlayerTotals(LeaguePlayers(leagueId), runId);
            var playerVBD = new Dictionary<LeaguePlayer, double>();

            var league = fantasyContext.Leagues.Single(l => l.Id == leagueId);
            foreach (var rp in league.RosterPositions)
            {
                //var posPlayers=rp.Players.
            }
            return null;
        }
    }
}
