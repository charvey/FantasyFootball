using FantasyFootball.Core;
using FantasyFootball.Core.Modeling;
using FantasyFootball.Core.Modeling.RosterModelers;
using FantasyFootball.Core.Modeling.ScoreModelers;
using FantasyFootball.Core.Objects;
using FantasyFootball.Terminal.Database;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace FantasyFootball.Terminal.Draft.Measures
{
    public class ValueAddedMeasure : Measure
    {
        private readonly SQLiteConnection connection;
        private readonly Draft draft;
        private readonly DraftParticipant participant;

        public ValueAddedMeasure(SQLiteConnection connection, Draft draft, DraftParticipant participant)
        {
            this.connection = connection;
            this.draft = draft;
            this.participant = participant;
        }

        public override string Name => "Value Added";

        public override int Width => Name.Length;

        public override IComparable Compute(Player player) => throw new NotImplementedException();

        public override IComparable[] Compute(Player[] players)
        {
            var currentPlayers = draft.PickedPlayersByParticipant(participant);
            var baseScore = GetTotalScore(currentPlayers);
            return players.Select(p => GetTotalScore(p.cons(currentPlayers)) - baseScore)
                .Cast<IComparable>()
                .ToArray();
        }

        private double GetTotalScore(IEnumerable<Player> players)
        {
            return Enumerable.Range(1, 16).Select(w => GetWeekScore(players, w)).Sum();
        }

        private readonly ConcurrentDictionary<Tuple<string, int>, double> predictions = new ConcurrentDictionary<Tuple<string, int>, double>();

        private double GetWeekScore(IEnumerable<Player> players, int week)
        {
            return new MostLikelyScoreRosterModeler(new RealityScoreModeler((p, w) => predictions.GetOrAdd(Tuple.Create(p.Id, week), t => connection.GetPrediction(t.Item1, 2017, t.Item2))))
                .Model(new RosterSituation(players.ToArray(), week))
                .Outcomes.Single().Players
                .Sum(p => predictions.GetOrAdd(Tuple.Create(p.Id, week), t => connection.GetPrediction(t.Item1, 2017, t.Item2)));
        }
    }

    public class DraftPickRecommendation
    {
        public Player Player { get; set; }
        public double ValueAdded { get; set; }
    }
}
