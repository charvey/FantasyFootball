using FantasyFootball.Core;
using FantasyFootball.Core.Draft;
using FantasyFootball.Core.Players;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Core.Calculators
{
    public class RegularSeasonWinsCalculator
    {
        private readonly RosterPicker rosterPicker;

        public int Calculate(Team team)
        {
            return Enumerable.Range(1, 13).Count(w => Win(team, w));
        }

        private bool Win(Team team, int w)
        {
            throw new NotImplementedException();
            //var matchup = Matchup.Matchups(w).Single(m => m.TeamA.Id == team.Id || m.TeamB.Id == team.Id);
            //var otherTeam = (matchup.TeamA.Id==team.Id)?
            //return GetWeekScore(team.Players, w) > GetWeekScore(otherTeam.Players, w);
        }

        private double GetWeekScore(IEnumerable<Player> players, int week)
        {
            return rosterPicker.PickRoster(players, week).Sum(p => Scores.GetScore(p, week));
        }
    }
}
