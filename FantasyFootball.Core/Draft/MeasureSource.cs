using FantasyFootball.Core.Data;
using FantasyFootball.Core.Draft.Measures;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Draft.Abstractions;
using FantasyPros;
using System;
using System.Collections.Concurrent;
using System.Linq;
using Yahoo;

namespace FantasyFootball.Core.Draft
{
    public static class MeasureSource
    {
        private static ConcurrentDictionary<LeagueKey, Measure[]> basicMeasures = new ConcurrentDictionary<LeagueKey, Measure[]>();
        public static Measure[] BasicMeasures(FantasySportsService service, LeagueKey leagueKey, IByeRepository byeRepository)
        {
            return basicMeasures.GetOrAdd(leagueKey, l_ => new Measure[] {
                new NameMeasure(),
                new TeamMeasure(),
                new PositionMeasure(),
                new ByeMeasure(byeRepository,service.League(leagueKey).season)
             });
        }

        private static ConcurrentDictionary<LeagueKey, Measure[]> draftMeasures = new ConcurrentDictionary<LeagueKey, Measure[]>();
        public static Measure[] DraftMeasures(FantasySportsService service, LeagueKey leagueKey, IByeRepository byeRepository, IDraft draft, FantasyProsClient fantasyProsClient)
        {
            return basicMeasures.GetOrAdd(leagueKey, l_ => new Measure[] {
                new NameMeasure(),
                new DraftedTeamMeasure(draft),
                new ADPMeasure(fantasyProsClient)
             });
        }

        private static ConcurrentDictionary<LeagueKey, Measure[]> predictionMeasures = new ConcurrentDictionary<LeagueKey, Measure[]>();
        public static Measure[] PredictionMeasures(FantasySportsService service, LeagueKey leagueKey, ILatestPredictionRepository predictionRepository)
        {
            return predictionMeasures.GetOrAdd(leagueKey, l_k =>
              new[] { new NameMeasure() }.Cast<Measure>()
                 .Concat(Enumerable.Range(1, service.League(leagueKey).end_week).Select(w => new WeekScoreMeasure(service, leagueKey, predictionRepository, w)))
                 .Concat(new[] { new TotalScoreMeasure(service, leagueKey, predictionRepository) })
                 .ToArray());
        }

        private static ConcurrentDictionary<LeagueKey, Measure[]> valueMeasures = new ConcurrentDictionary<LeagueKey, Measure[]>();
        public static Measure[] ValueMeasures(FantasySportsService service, IPlayerRepository playerRepository, ILatestPredictionRepository predictionRepository, LeagueKey leagueKey, int team_id, IDraft draft)
        {
            return valueMeasures.GetOrAdd(leagueKey, l_k => new Measure[] {
                new NameMeasure(),new PositionMeasure(),
                new FlexVBDMeasure(service, playerRepository, predictionRepository,leagueKey),
                new VBDMeasure(service, playerRepository, predictionRepository,leagueKey),
                new ValueAddedMeasure(service,leagueKey,predictionRepository,draft,draft.Participants.Single(p=>p.Name==service.Teams(leagueKey).Single(t=>t.team_id==team_id).name)),
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
        private readonly ILatestPredictionRepository predictionRepository;
        private readonly LeagueKey league_key;
        private readonly int week;

        public WeekScoreMeasure(FantasySportsService service, LeagueKey league_key, ILatestPredictionRepository predictionRepository, int week)
        {
            this.predictionRepository = predictionRepository;
            this.league_key = league_key;
            this.week = week;
        }

        public override string Name => $"Week {week}";
        public override IComparable Compute(Player player) => scores.GetOrAdd(player.Id, p => predictionRepository.GetPrediction(league_key, p, week));
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
