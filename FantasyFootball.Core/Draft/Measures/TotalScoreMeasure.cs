using FantasyFootball.Core.Data;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Yahoo;

namespace FantasyFootball.Core.Draft.Measures
{
    public class TotalScoreMeasure : Measure
    {
        private readonly ConcurrentDictionary<string, double> scores = new ConcurrentDictionary<string, double>();
        private readonly IPredictionRepository predictionRepository;
        private readonly LeagueKey leagueKey;

        public TotalScoreMeasure(FantasySportsService service, LeagueKey leagueKey, IPredictionRepository predictionRepository)
        {
            this.predictionRepository = predictionRepository;
            this.leagueKey = leagueKey;
        }

        private double GetScore(string playerId)
        {
            return predictionRepository.GetPredictions(leagueKey, playerId, Enumerable.Range(1, 17)).Sum();
        }

        public override string Name => "Total";
        public override IComparable Compute(Player player) => scores.GetOrAdd(player.Id, GetScore);
        public override int Width => 6;
    }
}
