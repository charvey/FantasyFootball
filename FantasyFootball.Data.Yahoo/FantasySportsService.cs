using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;

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
                return JsonConvert.DeserializeObject<FantasyContentWrapper>(json).fantasy_content.game;
            }
        }

        public IEnumerable<Player> Players(string game_key)
        {
            var json = webService.Players(game_key);
            Debugger.Break();
            return JsonConvert.DeserializeObject<FantasyContentWrapper>(json).fantasy_content.player;
        }
    }
}
