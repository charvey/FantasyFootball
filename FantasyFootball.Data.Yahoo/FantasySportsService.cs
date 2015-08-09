using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using FantasyFootball.Data.Yahoo.Actions;
using FantasyFootball.Data.Yahoo.Models;
using Newtonsoft.Json;

namespace FantasyFootball.Data.Yahoo
{
    public class FantasySportsService
    {
        private FantasySportsWebService webService = new FantasySportsWebService();

        public Game Game(string gameId)
        {
            var json = webService.Game(gameId);
            return JsonConvert.DeserializeObject<WebServiceResponse>(json).fantasy_content?.game.Single();
        }

        public IEnumerable<Game> Games()
        {
            if (!File.Exists("Year_GameId"))
                new YearGameIdMapBuilder().Build();
            var ids = File.ReadAllLines("Year_GameId").Select(x => x.Split(':')[1]);
            return Games(ids.ToArray());
        }

        public IEnumerable<Game> Games(params string[] gameIds)
        {
            return gameIds.Select(Game);
        }

        public League League(string league_key)
        {
            var json = webService.League(league_key);
            return JsonConvert.DeserializeObject<WebServiceResponse>(json).fantasy_content?.league.Single();
        }

        public IEnumerable<League> Leagues()
        {
            if (!File.Exists("LeagueKeys"))
                throw new NotImplementedException();
            var keys = File.ReadAllLines("LeagueKeys");
            return Leagues(keys.ToArray());
        }

        public IEnumerable<League> Leagues(params string[] league_keys)
        {
            return league_keys.Select(League);
        }

        public IEnumerable<Player> Players(string game_key)
        {
            var json = webService.Players(game_key);
            return JsonConvert.DeserializeObject<WebServiceResponse>(json).fantasy_content.player;
        }
    }
}
