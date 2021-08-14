using BarstoolSportsBook;
using FantasyFootball.Preseason.Abstractions;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace FantasyFootball.Preseason.BsbOddsClient
{
    public class BarstoolSportsBookOddsClient : OddsClient
    {
        private readonly BarstoolSportsBookClient client;

        public BarstoolSportsBookOddsClient(BarstoolSportsBookClient client)
        {
            this.client = client;
        }

        public async Task<IEnumerable<GameOdds>> GetOdds()
        {
            var raw = await client.Get("american_football/nfl_preseason");

            return raw.Events.Select(Convert);
        }

        private GameOdds Convert(Event @event)
        {
            var moneylines = @event.BetOffers.Single(b => b.BetOfferType.Name == "Match")
                .Outcomes.ToDictionary(o => o.participant, o => o);
            var spreads = @event.BetOffers.Single(b => b.BetOfferType.Name == "Handicap")
                .Outcomes.ToDictionary(o => o.participant, o => o);

            var participants = moneylines.Keys.Union(spreads.Keys);

            Debug.Assert(participants.Count() == 2);

            return new GameOdds(participants.Select(p => new TeamOdds(
                p,
                moneylines[p].odds / 100.0,
                spreads[p].line.Value / 100.0m,
                spreads[p].odds / 100.0
            )).ToArray());
        }
    }
}
