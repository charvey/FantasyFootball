using FantasyFootball.Core.Players;
using System.Linq;

namespace FantasyFootball.Core.Draft
{
    public static class DraftExtensions
    {
        public static Team GetNextDraftTeam(this Draft draft)
        {
            foreach (var round in Enumerable.Range(1, 15))
            {
                var order = round % 2 == 1
                    ? draft.Teams.AsEnumerable()
                    : draft.Teams.AsEnumerable().Reverse();

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

        public static int? GetNextDraftRound(this Draft draft)
        {
            foreach (var round in Enumerable.Range(1, 15))
            {
                var order = round % 2 == 1
                    ? draft.Teams.AsEnumerable()
                    : draft.Teams.AsEnumerable().Reverse();

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
