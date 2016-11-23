using FantasyFootball.Core.Draft;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Rosters
{
    public class RosterHelper
    {
        public void Help(TextWriter output)
        {
            int week = 1;
            var team = Draft.Draft.FromFile().PickedPlayersByTeam(new Team { Id = 7 });
            var rosterPicker = new RosterPicker(new DataCsvScoreProvider());
            var roster = rosterPicker.PickRoster(team, week);
            foreach(var player in roster)
            {
                output.WriteLine(player.Name + " " + player.Position);
            }
            output.WriteLine(roster.Sum(p => Scores.GetScore(p, week)));
        }
    }
}
