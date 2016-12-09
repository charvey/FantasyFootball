using FantasyFootball.Core.Objects;
using FantasyFootball.Core.Simulation.Projections;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace FantasyFootball.Core.Simulation
{
    public static class StandingsExtensions
    {
        public static int GetSeedAtEndOfSeason(this Universe universe, Team team)
        {
            return universe.GetStandingsAtEndOfSeason().ToList().IndexOf(team) + 1;
        }

        public static Team GetTeamInPlaceAtEndOfSeason(this Universe universe, int place)
        {
            return universe.GetStandingsAtEndOfSeason()[place - 1];
        }

        public static Team[] GetStandingsAfterWeek(this Universe universe, int week)
        {
            var teams = universe.GetTeams()
                .OrderByDescending(t => universe.GetRecordAfterWeek(t, week))
                .ThenByDescending(t => universe.TotalScoreAfterWeek(t, week));

            //foreach (var w in SeasonWeek.RegularSeasonWeeks.Reverse())
            //{
            //    teams = teams.ThenByDescending(t => universe.GetScore(t, w));
            //}

            //teams = teams.ThenByDescending(t => new Random().NextDouble());

            return teams.ToArray();
        }

        private static ConcurrentDictionary<Guid, Team[]> standingsCache = new ConcurrentDictionary<Guid, Team[]>();
        public static Team[] GetStandingsAtEndOfSeason(this Universe universe)
        {
            return standingsCache.GetOrAdd(universe.Id, _ => universe.GetStandingsAfterWeek(SeasonWeek.RegularSeasonEnd));
        }

        public static Record GetRecordAfterWeek(this Universe universe, Team team, int week)
        {
            var allRegularSeasonMatchups = Enumerable.Range(1, week).Select(w => MatchupProjection.GetMatchups(universe, w));
            var matchupsWithTeam = allRegularSeasonMatchups
                .SelectMany(wm => wm.Where(m => m.TeamA.Id == team.Id || m.TeamB.Id == team.Id));
            var results = matchupsWithTeam.Select(m => universe.GetRegularSeasonResult(m)).ToArray();
            var wins = results.Count(r => r.Winner == team);
            var losses = results.Count(r => r.Loser == team);
            var ties = results.Count(r => r.Tied);
            return new Record(wins, losses, ties);
        }

        public static double TotalScoreAfterWeek(this Universe universe, Team team, int week)
        {
            return Enumerable.Range(1, week).Sum(w => universe.GetScore(team, w));
        }

        public class Record : IComparable<Record>
        {
            public int Wins { get; set; }
            public int Losses { get; set; }
            public int Ties { get; set; }

            private int TotalGames => Wins + Losses + Ties;

            public Record(int wins, int losses, int ties)
            {
                this.Wins = wins;
                this.Losses = losses;
                this.Ties = ties;
            }

            public int CompareTo(Record other)
            {
                if (TotalGames != other.TotalGames)
                    throw new InvalidOperationException();

                if (Wins < other.Wins) return -1;
                else if (Wins > other.Wins) return 1;

                if (Losses > other.Losses) return -1;
                else if (Losses < other.Losses) return 1;

                if (other.Wins == Wins && other.Losses == Losses) return 0;

                throw new NotImplementedException();
            }
        }
    }
}
