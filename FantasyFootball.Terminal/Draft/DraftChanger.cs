﻿using FantasyFootball.Draft.Abstractions;

namespace FantasyFootball.Terminal.Draft
{
    public class DraftChanger
    {
        private readonly TextReader input;
        private readonly TextWriter output;

        public DraftChanger(TextReader input, TextWriter output)
        {
            this.input = input;
            this.output = output;
        }

        public void Change(IDraft draft)
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
