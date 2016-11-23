using FantasyFootball.Core.Players;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FantasyFootball.Core.Draft
{
    public class DraftWriter
    {
        private int ColumnWidth
        {
            get
            {
                int teams = 12;// draft.Teams.Count();
                return (Console.BufferWidth - teams - 5) / teams;
            }
        }

        public void WriteDraft(TextWriter writer, Draft draft)
        {
            writer.WriteLine(BuildRow("Round", draft.Teams.Select(t => t.Owner)));
            writer.WriteLine(BuildRow(string.Empty, draft.Teams.Select(t => t.Name)));

            for (int r = 1; r <= 15; r++)
            {
                writer.WriteLine(BuildRow(string.Empty, draft.Teams.Select(t => (draft.Pick(t, r)?.Id ?? ""))));
                writer.WriteLine(BuildRow(" #" + r.ToString(), draft.Teams.Select(t => (draft.Pick(t, r)?.Name ?? ""))));
                writer.WriteLine(BuildRow(string.Empty, draft.Teams.Select(t => (draft.Pick(t, r)?.Position ?? "" )+" "+ (draft.Pick(t, r)?.Team ?? ""))));
            }
        }

        private string BuildRow(string leftColumn, IEnumerable<string> otherColumns)
        {
            return string.Join("|", new[] { PadAndCut(leftColumn, 5) }.Concat(otherColumns.Select(x => PadAndCut(x, ColumnWidth))));
        }

        private string PadAndCut(string source,int length)
        {
            return source.PadRight(length).Substring(0, length);
        }
    }
}
