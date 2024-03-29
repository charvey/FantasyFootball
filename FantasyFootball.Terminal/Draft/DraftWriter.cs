﻿using FantasyFootball.Draft.Abstractions;

namespace FantasyFootball.Terminal.Draft
{
    public class DraftWriter
    {
        private readonly TextWriter writer;

        public DraftWriter(TextWriter writer)
        {
            this.writer = writer;
        }

        private int ColumnWidth
        {
            get
            {
                int teams = 12;// draft.Teams.Count();
                return (Console.BufferWidth - teams - 5) / teams;
            }
        }

        public void WriteDraft(IDraft draft)
        {
            var participants = draft.Participants.OrderBy(p => p.Order);
            writer.WriteLine(BuildRow("Round", participants.Select(t => t.Owner)));
            writer.WriteLine(BuildRow(string.Empty, participants.Select(t => t.Name)));

            for (int r = 1; r <= 15; r++)
            {
                writer.WriteLine(BuildRow(string.Empty, participants.Select(t => (draft.Pick(t, r)?.Id ?? ""))));
                writer.WriteLine(BuildRow(" #" + r.ToString(), participants.Select(t => (draft.Pick(t, r)?.Name ?? ""))));
                writer.WriteLine(BuildRow(string.Empty, participants.Select(t => string.Join("/", draft.Pick(t, r)?.Positions ?? new string[0]) + " " + (draft.Pick(t, r)?.Team ?? ""))));
            }
        }

        private string BuildRow(string leftColumn, IEnumerable<string> otherColumns)
        {
            return string.Join("|", new[] { PadAndCut(leftColumn, 5) }.Concat(otherColumns.Select(x => PadAndCut(x ?? string.Empty, ColumnWidth))));
        }

        private string PadAndCut(string source, int length)
        {
            return source.PadRight(length).Substring(0, length);
        }
    }
}
