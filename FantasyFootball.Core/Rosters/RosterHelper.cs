using FantasyFootball.Core.Draft;
using FantasyFootball.Core.Objects;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Rosters
{
    public class RosterHelper
    {
        public void Help(TextWriter output)
        {
            int week = SeasonWeek.Current;
            var team = Draft.Draft.FromFile().PickedPlayersByTeam(new Team { Id = 7 });
            var rosterPicker = new RosterPicker(new DataCsvScoreProvider());
            var roster = rosterPicker.PickRoster(team, week);
            foreach(var player in roster)
            {
                output.WriteLine(player.Name + " " + player.Positions);
            }
            output.WriteLine(roster.Sum(p => Scores.GetScore(p, week)));
        }
    }
}
