using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling;
using FantasyFootball.Core.Modeling.RosterModelers;
using FantasyFootball.Core.Modeling.ScoreModelers;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Rosters
{
    public class RosterHelper
    {
        public void Help(TextWriter output, Func<Player, int, double> scores, string league_key, int team_id)
        {
            var service = new FantasySportsService();
            var week = service.League(league_key).current_week;
            var players = service.TeamRoster($"{league_key}.t.{team_id}", week).players.Select(Players.From).ToArray();
            var rosterPicker = new MostLikelyScoreRosterModeler(new RealityScoreModeler(scores));
            var roster = rosterPicker.Model(new RosterSituation(players, week));
            foreach (var player in roster.Outcomes.Single().Players)
            {
                output.WriteLine($"{scores(player, week):00.00} {player.Name} {string.Join("/", player.Positions)}");
            }
            output.WriteLine(roster.Outcomes.Single().Players.Sum(p => scores(p, week)));
        }
    }
}
