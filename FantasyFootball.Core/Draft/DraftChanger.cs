using FantasyFootball.Core.Players;
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
            var players = Player.All().Except(draft.PickedPlayers);
            while (players.Count() > 1)
            {
                Console.Clear();
                foreach (var p in players.Take(30))
                    Console.WriteLine(p.Id + " " + p.Name + " " + p.Position + " " + p.Team);

                Console.WriteLine();
                Console.WriteLine(team.Owner + " is picking");
                Console.WriteLine("Enter a filter:");
                var filter = Console.ReadLine().ToLower();
                players = players.Where(p => (p.Id + " " + p.Name + " " + p.Position + " " + p.Team).ToLower().Contains(filter));
            }
            var player = players.Single();
            Console.WriteLine(team.Owner + " picks " + player.Name + " from " + player.Team + " as " + player.Position);
            draft.Pick(team, draft.GetNextDraftRound().Value, player);            
        }
    }
}
