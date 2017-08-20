using System;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Draft
{
    public class DraftChanger
    {
        public void Change(TextWriter output, TextReader input, Draft draft)
        {
            var team = draft.GetNextDraftTeam();
            var players = draft.UnpickedPlayers.AsEnumerable();
            while (players.Count() > 1)
            {
                Console.Clear();
                foreach (var p in players.Take(30))
                    output.WriteLine(p.Id + " " + p.Name + " " + string.Join(",", p.Positions) + " " + p.Team);

                output.WriteLine();
                output.WriteLine(team.Owner + " is picking");
                output.WriteLine("Enter a filter:");
                var filter = input.ReadLine().ToLower();
                players = players.Where(p => (p.Id + " " + p.Name + " " + string.Join(",", p.Positions) + " " + p.Team).ToLower().Contains(filter));
            }
            var player = players.Single();
            output.WriteLine(team.Owner + " picks " + player.Name + " from " + player.Team + " as " + string.Join(",", player.Positions));
            draft.Pick(team, draft.GetNextDraftRound().Value, player);            
        }
    }
}
