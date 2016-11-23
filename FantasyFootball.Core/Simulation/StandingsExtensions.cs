using FantasyFootball.Core.Draft;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace FantasyFootball.Core.Simulation
{
    public static class StandingsExtensions
    {        
        public static Team GetTeamInPlaceAtEndOfSeason(this Universe universe, int place)
        {
            return universe.GetStandingsAtEndOfSeason()[place - 1];
        }

        public static Team[] GetStandingsAfterWeek(this Universe universe, int week)
        {
            return universe
                .GetTeams()
                .OrderByDescending(t => universe.GetRecordAfterWeek(t, week))
                .ThenByDescending(t => universe.TotalScoreAfterWeek(t, week))
                //.ThenByDescending(universe.LastWeekScore)
                .ToArray();
        }

        private static ConcurrentDictionary<Guid, Team[]> standingsCache = new ConcurrentDictionary<Guid, Team[]>();
        private static Team[] GetStandingsAtEndOfSeason(this Universe universe)
        {
            return standingsCache.GetOrAdd(universe.Id, _ => universe.GetStandingsAfterWeek(13));
        }

        public static Record GetRecordAfterWeek(this Universe universe, Team team, int week)
        {
            var allRegularSeasonMatchups = Enumerable.Range(1, week).Select(universe.GetMatchups);
            var matchupsWithTeam = allRegularSeasonMatchups
                .SelectMany(wm => wm.Where(m => m.TeamA.Id == team.Id || m.TeamB.Id == team.Id));
            var winners = matchupsWithTeam.Select(m => universe.GetWinner(m)).ToArray();
            var wins = winners.Count(w => w.Id == team.Id);
            var losses = winners.Count(w => w.Id != team.Id);
            var ties = winners.Count() - (wins + losses);
            return new Record(wins, losses, ties);
        }

        public static double TotalScoreAfterWeek(this Universe universe, Team team, int week)
        {
            return Enumerable.Range(1, week).Sum(w => universe.GetScore(team, w));
        }

        private static double LastWeekScore(this Universe universe, Team team)
        {
            //If needed add stub for next tie breaker
            //https://help.yahoo.com/kb/head-article-non-divisional-leagues-sln6446.html
            throw new NotImplementedException();
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
