using System.Collections.Generic;
using System.Diagnostics;
using FantasyFootball.Data.Yahoo.Models;
using Newtonsoft.Json;

namespace FantasyFootball.Data.Yahoo
{
    public class FantasySportsService
    {
        private FantasySportsWebService webService = new FantasySportsWebService();

        public IEnumerable<Game> Games
        {
            get
            {
                var json = webService.Games();
                Debugger.Break();
                return JsonConvert.DeserializeObject<WebServiceResponse>(json).fantasy_content.game;
            }
        }

        public IEnumerable<Game> Game(string gameId)
        {
            var json = webService.Game(gameId);
            return JsonConvert.DeserializeObject<WebServiceResponse>(json).fantasy_content?.game;
        }

        public IEnumerable<Player> Players(string game_key)
        {
            var json = webService.Players(game_key);
            return JsonConvert.DeserializeObject<WebServiceResponse>(json).fantasy_content.player;
        }
    }
}
