using FantasyFootball.Core.Draft;
using FantasyFootball.Draft.Abstractions;

namespace FantasyFootball.Terminal.Draft
{
    public class DraftDataWriter
    {
        public void WriteData(IDraft draft, Measure[] measures)
        {
            var players = draft.UnpickedPlayers.ToArray();

            var columns = measures.Select(m => m.Compute(players)).ToArray();

            var data = Enumerable.Range(0, players.Length)
                .Select(i => columns.Select(c => c[i]).ToArray());

            data = data.OrderByDescending(p => p.Last()).ToList();

            Console.WriteLine(string.Join("|", measures.Select(m => PadAndCut(m.Name, m.Width))));
            foreach (var row in data)
                Console.WriteLine(string.Join("|", row.Select((c, i) => PadAndCut(c.ToString(), measures[i].Width))));
        }

        private string PadAndCut(string source, int length)
        {
            return source.PadRight(length).Substring(0, length);
        }
    }
}
