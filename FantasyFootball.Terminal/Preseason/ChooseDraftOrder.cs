﻿using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling;
using FantasyFootball.Core.Modeling.RosterModelers;
using FantasyFootball.Core.Modeling.ScoreModelers;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Terminal.Database;
using System;
using System.Collections.Generic;
using System.Linq;
using Yahoo;

namespace FantasyFootball.Terminal.Preseason
{
    public static class ChooseDraftOrder
    {
        public static void Do(FantasySportsService service, IPlayerRepository playerRepository, ILatestPredictionRepository predictionRepository, LeagueKey leagueKey)
        {
            var players = service.LeaguePlayers(leagueKey)
                    .Select(p => playerRepository.GetPlayer(p.player_id.ToString()))
                    .PutInDraftOrder(predictionRepository, leagueKey)
                    .ToList();
            var teams = service.League(leagueKey).num_teams;
            var rounds = service.LeagueSettings(leagueKey)
                .roster_positions.Sum(rp => rp.count);

            var results = Enumerable.Range(0, teams).Select(_ => new List<Player>()).ToArray();

            for (var round = 1; round <= rounds; round++)
            {
                var pickedPlayers = players.Skip((round - 1) * teams).Take(teams);
                if (round % 2 == 0)
                    pickedPlayers = pickedPlayers.Reverse();

                for (var team = 0; team < teams; team++)
                    results[team].Add(pickedPlayers.ElementAt(team));
            }

            for (var position = 1; position <= teams; position++)
            {
                Console.WriteLine($"{position:#0}. {results[position - 1].Evaluate(predictionRepository, leagueKey)}");
            }
        }

        private static IEnumerable<Player> PutInDraftOrder(this IEnumerable<Player> players, ILatestPredictionRepository predictionRepository, LeagueKey leagueKey)
        {
            var scores = players
                .ToDictionary(p => p.Id, p => GetScore(predictionRepository, leagueKey, p.Id));
            var replacementScores = players
                .SelectMany(p => p.Positions.Select(pos => Tuple.Create(pos, p)))
                .GroupBy(p => p.Item1, p => scores[p.Item2.Id])
                .ToDictionary(g => g.Key, ComputeReplacement);
            return players
                .OrderByDescending(p => scores[p.Id] - p.Positions.Min(pos => replacementScores[pos]));
        }

        #region VBD
        private static double ComputeReplacement(IGrouping<string, double> group)
        {
            var scores = group.OrderByDescending(x => x);
            int count;
            switch (group.Key)
            {
                case "QB":
                case "TE":
                case "K":
                case "DEF":
                    count = 12;
                    break;
                case "WR":
                case "RB":
                    count = 24;
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
            return scores.Skip(count - 1).First();
        }

        private static double GetScore(ILatestPredictionRepository predictionRepository, LeagueKey leagueKey, string playerId)
        {
            return predictionRepository.GetPredictions(leagueKey, playerId, Enumerable.Range(1, 16)).Sum();
        }
        #endregion

        private static double Evaluate(this List<Player> team, ILatestPredictionRepository predictionRepository, LeagueKey leagueKey)
        {
            return Enumerable.Range(1, 16).Select(w => GetWeekScore(predictionRepository, leagueKey, team, w)).Sum();
        }

        private static double GetWeekScore(ILatestPredictionRepository predictionRepository, LeagueKey leagueKey, IEnumerable<Player> players, int week)
        {
            return new MostLikelyScoreRosterModeler(new RealityScoreModeler((p, w) => predictionRepository.GetPrediction(leagueKey, p.Id, w)))
                .Model(new RosterSituation(players.ToArray(), week))
                .Outcomes.Single().Players
                .Sum(p => predictionRepository.GetPrediction(leagueKey, p.Id, week));
        }
    }
}
