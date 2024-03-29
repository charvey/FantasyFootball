﻿using FantasyFootball.Draft.Abstractions;

namespace FantasyFootball.Terminal.Draft
{
    public static class DraftExtensions
    {
        public static DraftParticipant GetNextDraftTeam(this IDraft draft)
        {
            foreach (var round in Enumerable.Range(1, 15))
            {
                var order = round % 2 == 1
                    ? draft.Participants.AsEnumerable()
                    : draft.Participants.AsEnumerable().Reverse();

                foreach (var team in order)
                {
                    if (draft.Pick(team, round) == null)
                    {
                        return team;
                    }
                }
            }
            return null;
        }

        public static int? GetNextDraftRound(this IDraft draft)
        {
            foreach (var round in Enumerable.Range(1, 15))
            {
                var order = round % 2 == 1
                    ? draft.Participants.AsEnumerable()
                    : draft.Participants.AsEnumerable().Reverse();

                foreach (var team in order)
                {
                    if (draft.Pick(team, round) == null)
                    {
                        return round;
                    }
                }
            }
            return null;
        }
    }
}
