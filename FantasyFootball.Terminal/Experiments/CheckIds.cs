using ClickyDraft;
using FantasyFootball.Core.Data;
using FantasyFootball.Data.Yahoo;
using FantasyFootball.Draft.ClickyDraft;
using Yahoo;

namespace FantasyFootball.Terminal.Experiments
{
    class CheckIds
    {
        public const string PREFIX = ClickyDraftId.PREFIX;

        public void Run(LeagueKey league_key, FantasySportsService fantasySportsService, ILatestPredictionRepository predictionRepository)
        {
            var draft = new ClickyDraftDraft(DemoLeagueIds.LeagueId, DemoLeagueIds.LeagueInstanceId);
            var players = fantasySportsService.LeaguePlayers(league_key);

            var draftPlayers = draft.AllPlayers;
            var draftPlayerIds = new HashSet<string>(draftPlayers.Select(p => p.Id));

            var missingPlayers = players.Where(p => !draftPlayerIds.Contains(p.player_id.ToString()));

            foreach (var player in missingPlayers)
            {
                var matches = draftPlayers.Where(p => p.Id.StartsWith(PREFIX)).Where(p => player.name.full == p.Name);

                if (matches.Count() == 1)
                {
                    var match = matches.Single();
                    var pair = $"{{{match.Id.Substring(PREFIX.Length)},{player.player_id}}}";
                    Console.WriteLine($"Matched {player.name.full} {pair}");
                    File.AppendAllLines("ids", new[] { $"{pair}," });
                }
                else if (matches.Count() == 0)
                {
                    var total = predictionRepository.GetPredictions(league_key, player.player_id.ToString(), Enumerable.Range(1, 17)).Sum();

                    Console.WriteLine($"Missing {player.name.full} {total}");
                }
                else
                {
                    Console.WriteLine($"Found {matches.Count()} {player.name.full}");
                }
            }
        }
    }
}
