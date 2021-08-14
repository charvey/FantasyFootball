using FantasyFootball.Preseason.Abstractions;
using System.Collections.Generic;
using System.Linq;

namespace FantasyFootball.Preseason
{
    public class PreseasonOddsRecommender
    {
        public record Recommendation(string Name);

        private readonly OddsClient oddsClient;

        public PreseasonOddsRecommender(OddsClient oddsClient)
        {
            this.oddsClient = oddsClient;
        }

        public IEnumerable<Recommendation> GetRecommendations()
        {
            return SortByOdds(oddsClient.GetOdds().Result.Select(Favorite))
                .Select(t => new Recommendation(t.Name));
        }

        private TeamOdds Favorite(GameOdds game)
        {
            return SortByOdds(game.Teams).First();
        }

        private IEnumerable<TeamOdds> SortByOdds(IEnumerable<TeamOdds> odds)
        {
            return odds
                .OrderBy(t => t.MoneyLineOdds)
                .ThenBy(t => t.Spread)
                .ThenBy(t => t.SpreadOdds);
        }
    }
}
