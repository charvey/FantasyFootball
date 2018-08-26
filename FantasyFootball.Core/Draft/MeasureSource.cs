using FantasyFootball.Core.Data;
using FantasyFootball.Core.Draft.Measures;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using FantasyPros;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Core.Draft
{
    public static class MeasureSource
    {
        private static ConcurrentDictionary<string, Measure[]> basicMeasures = new ConcurrentDictionary<string, Measure[]>();
        public static Measure[] BasicMeasures(FantasySportsService service, string league_key, IByeRepository byeRepository)
        {
            return basicMeasures.GetOrAdd(league_key, l_ => new Measure[] {
                new NameMeasure(),
                new TeamMeasure(),
                new PositionMeasure(),
                new ByeMeasure(byeRepository,service.League(league_key).season)
             });
        }

        private static ConcurrentDictionary<string, Measure[]> draftMeasures = new ConcurrentDictionary<string, Measure[]>();
        public static Measure[] DraftMeasures(FantasySportsService service, string league_key, IByeRepository byeRepository, IDraft draft, FantasyProsClient fantasyProsClient)
        {
            return basicMeasures.GetOrAdd(league_key, l_ => new Measure[] {
                new NameMeasure(),
                new DraftedTeamMeasure(draft),
                new ADPMeasure(fantasyProsClient)
             });
        }

        private static ConcurrentDictionary<string, Measure[]> predictionMeasures = new ConcurrentDictionary<string, Measure[]>();
        public static Measure[] PredictionMeasures(FantasySportsService service, string league_key, IPredictionRepository predictionRepository)
        {
            return predictionMeasures.GetOrAdd(league_key, l_k =>
              new[] { new NameMeasure() }.Cast<Measure>()
                 .Concat(Enumerable.Range(1, 17).Select(w => new WeekScoreMeasure(service,league_key, predictionRepository, w)))
                 .Concat(new[] { new TotalScoreMeasure(service,league_key, predictionRepository) })
                 .ToArray());
        }

        private static ConcurrentDictionary<string, Measure[]> valueMeasures = new ConcurrentDictionary<string, Measure[]>();
        public static Measure[] ValueMeasures(FantasySportsService service, IPlayerRepository playerRepository, IPredictionRepository predictionRepository, string league_key, IDraft draft)
        {
            return valueMeasures.GetOrAdd(league_key, l_k => new Measure[] {
                new NameMeasure(),new PositionMeasure(),
                new FlexVBDMeasure(service, playerRepository, predictionRepository,league_key),
                new VBDMeasure(service, playerRepository, predictionRepository,league_key),
                new ValueAddedMeasure(service,league_key,predictionRepository,draft,draft.Participants.Single(p=>p.Name=="Money Ballers")),
            });
        }
    }

    public class TeamMeasure : Measure
    {
        public override string Name => "Team";
        public override IComparable Compute(Player player) => player.Team;
        public override int Width => 4;
    }

    public class PositionMeasure : Measure
    {
        public override string Name => "Position";
        public override IComparable Compute(Player player) => string.Join("/", player.Positions);
        public override int Width => 5;
    }

    public class WeekScoreMeasure : Measure
    {
        private readonly ConcurrentDictionary<string, double> scores = new ConcurrentDictionary<string, double>();
        private readonly IPredictionRepository predictionRepository;
        private readonly int week;
        private readonly int year;

        public WeekScoreMeasure(FantasySportsService service, string league_key, IPredictionRepository predictionRepository, int week)
        {
            this.predictionRepository = predictionRepository;
            this.week = week;
            this.year = service.League(league_key).season;
        }

        public override string Name => $"Week {week}";
        public override IComparable Compute(Player player) => scores.GetOrAdd(player.Id, p => predictionRepository.GetPrediction(p, year, week));
        public override int Width => Math.Min(6, Name.Length);
    }

    public class ByeMeasure : Measure
    {
        private readonly ConcurrentDictionary<string, int> byes = new ConcurrentDictionary<string, int>();
        private readonly IByeRepository byeRepository;
        private readonly int year;

        public ByeMeasure(IByeRepository byeRepository, int year)
        {
            this.byeRepository = byeRepository;
            this.year = year;
        }

        public override string Name => "Bye Week";
        public override IComparable Compute(Player player) => byes.GetOrAdd(player.Team, t => byeRepository.GetByeWeek(year, t));
        public override int Width => 3;
    }
}
