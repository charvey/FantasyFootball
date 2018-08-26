using FantasyFootball.Core.Data;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace FantasyFootball.Core.Draft.Measures
{
    public class TotalScoreMeasure : Measure
    {
        private readonly ConcurrentDictionary<string, double> scores = new ConcurrentDictionary<string, double>();
        private readonly IPredictionRepository predictionRepository;
        private readonly int year;

        public TotalScoreMeasure(FantasySportsService service, string league_key, IPredictionRepository predictionRepository)
        {
            this.predictionRepository = predictionRepository;
            this.year = service.League(league_key).season;
        }

        private double GetScore(string playerId)
        {
            return predictionRepository.GetPredictions(playerId, year, Enumerable.Range(1, 17)).Sum();
        }

        public override string Name => "Total";
        public override IComparable Compute(Player player) => scores.GetOrAdd(player.Id, GetScore);
        public override int Width => 6;
    }
}
