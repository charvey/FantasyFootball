using FantasyFootball.Core.Data;
using FantasyFootball.Core.Modeling;
using FantasyFootball.Core.Modeling.RosterModelers;
using FantasyFootball.Core.Modeling.ScoreModelers;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Rosters
{
    public class RosterHelper
    {
        public void Help(TextWriter output, string league_key, int team_id)
        {
            var service = new FantasySportsService();
            var week = service.League(league_key).current_week;
            var players = service.TeamRoster($"{league_key}.t.{team_id}", week).players.Select(Players.From).ToArray();
            var rosterPicker = new MostLikelyScoreRosterModeler(new RealityScoreModeler());
            var roster = rosterPicker.Model(new RosterSituation(players, week));
            foreach (var player in roster.Outcomes.Single().Players)
            {
                output.WriteLine(player.Name + " " + string.Join("/", player.Positions));
            }
            output.WriteLine(roster.Outcomes.Single().Players.Sum(p => DumpData.GetScore(p, week)));
        }
    }
}
