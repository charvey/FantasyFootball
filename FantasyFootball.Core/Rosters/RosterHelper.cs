using FantasyFootball.Core.Data;
using FantasyFootball.Core.Objects;
using FantasyFootball.Data.Yahoo;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Rosters
{
    public class RosterHelper
    {
        private const string league_key = "359.l.48793";

        public void Help(TextWriter output)
        {
            var service = new FantasySportsService();
            var week = SeasonWeek.Current;
            var players = service.TeamRoster($"{league_key}.t.{7}", week).players.Select(Players.From).ToArray();
            var rosterPicker = new RosterPicker(new DumpCsvScoreProvider());
            var roster = rosterPicker.PickRoster(players, week);
            foreach(var player in roster)
            {
                output.WriteLine(player.Name + " " + string.Join("/", player.Positions));
            }
            output.WriteLine(roster.Sum(p => DumpData.GetScore(p, week)));
        }
    }
}
