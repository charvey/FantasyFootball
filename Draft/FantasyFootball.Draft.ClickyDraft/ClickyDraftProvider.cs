using ClickyDraft;
using FantasyFootball.Draft.Abstractions;

namespace FantasyFootball.Draft.ClickyDraft
{
    public class ClickyDraftProvider : IDraftProvider
    {
        private readonly ClickyDraftService clickyDraftService = new ClickyDraftService();

        private record LeagueIds(int LeagueId, int LeagueInstanceId);

        public IReadOnlyList<IDraftProvider.DraftEntry> GetDrafts()
        {
            return new LeagueIds[]
            {
                new(DemoLeagueIds.LeagueId,DemoLeagueIds.LeagueInstanceId),
            }.Select(lid =>
            {
                var league = clickyDraftService.League(lid.LeagueId, lid.LeagueInstanceId);

                return new IDraftProvider.DraftEntry(league.DisplayName, () => new ClickyDraftDraft(lid.LeagueId, lid.LeagueInstanceId));
            }).ToList();
        }
    }
}
