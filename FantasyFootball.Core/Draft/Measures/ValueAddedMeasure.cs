using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling;
using FantasyFootball.Core.Modeling.RosterModelers;
using FantasyFootball.Core.Modeling.ScoreModelers;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Core.Draft.Measures
{
    public class ValueAddedMeasure : Measure
    {
        private readonly IPredictionRepository predictionRepository;
        private readonly Func<IReadOnlyList<Player>> currentPlayersFactory;
        private readonly DraftParticipant draftParticipant;
        private readonly IEnumerable<int> weeks;
        private readonly int year;

        public ValueAddedMeasure(FantasySportsService service, string league_key, IPredictionRepository predictionRepository, IDraft draft, DraftParticipant participant)
        {
            this.predictionRepository = predictionRepository;
            this.draftParticipant = participant;
            this.currentPlayersFactory = () => draft.PickedPlayersByParticipant(participant);
            this.weeks = Enumerable.Range(1, SeasonWeek.ChampionshipWeek);
            this.year = service.League(league_key).season;
        }

        public override string Name => $"Value Added for {draftParticipant.Name}";

        public override int Width => Name.Length;

        public override IComparable Compute(Player player)
        {
            var currentPlayers = currentPlayersFactory();
            return GetTotalScore(player.cons(currentPlayers), year) - GetTotalScore(currentPlayers, year);
        }

        public override IComparable[] Compute(Player[] players)
        {
            var currentPlayers = currentPlayersFactory();
            var baseScore = GetTotalScore(currentPlayers, year);
            return players.Select(p => GetTotalScore(p.cons(currentPlayers), year) - baseScore)
                .Cast<IComparable>()
                .ToArray();
        }

        private double GetTotalScore(IEnumerable<Player> players, int year)
        {
            return weeks.Select(w => GetWeekScore(players, year, w)).Sum();
        }

        private double GetWeekScore(IEnumerable<Player> players, int year, int week)
        {
            return new MostLikelyScoreRosterModeler(new RealityScoreModeler((p, w) => predictionRepository.GetPrediction(p.Id, year, week)))
                .Model(new RosterSituation(players.ToArray(), week))
                .Outcomes.Single().Players
                .Sum(p => predictionRepository.GetPrediction(p.Id, year, week));
        }
    }

    public class DraftPickRecommendation
    {
        public Player Player { get; set; }
        public double ValueAdded { get; set; }
    }
}
