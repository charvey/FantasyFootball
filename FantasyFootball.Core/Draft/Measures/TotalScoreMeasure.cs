using FantasyFootball.Core.Data;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Draft.Abstractions;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Yahoo;

namespace FantasyFootball.Core.Draft.Measures
{
    public class TotalScoreMeasure : Measure
    {
        private readonly ConcurrentDictionary<string, double> scores = new ConcurrentDictionary<string, double>();
        private readonly ILatestPredictionRepository predictionRepository;
        private readonly FantasySportsService service;
        private readonly LeagueKey leagueKey;

        public TotalScoreMeasure(FantasySportsService service, LeagueKey leagueKey, ILatestPredictionRepository predictionRepository)
        {
            this.predictionRepository = predictionRepository;
            this.leagueKey = leagueKey;
            this.service = service;
        }

        private double GetScore(string playerId)
        {
            return predictionRepository.GetPredictions(leagueKey, playerId, Enumerable.Range(1, service.League(leagueKey).end_week)).Sum();
        }

        public override string Name => "Total";
        public override IComparable Compute(Player player) => scores.GetOrAdd(player.Id, GetScore);
        public override int Width => 6;
    }
}
