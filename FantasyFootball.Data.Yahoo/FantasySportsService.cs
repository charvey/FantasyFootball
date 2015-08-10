using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FantasyFootball.Data.Yahoo.Actions;
using FantasyFootball.Data.Yahoo.Models;
using Newtonsoft.Json;
using System.Xml.XPath;

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

        public Team Team(string team_key)
        {
            var xml = webService.Team(team_key);
            return XmlConvert.Deserialize<FantasyContent>(xml)?.team;
        }

        public IEnumerable<Team> Teams()
        {
            var league = Leagues().Single(l => l.season == DateTime.Now.Year);
            return Teams(league.league_key);
        }

        public IEnumerable<Team> Teams(string league_key)
        {
            var xml = webService.Teams(league_key);
            var doc = XDocument.Parse(xml);
            var teams = doc.XPathSelectElements("//*[local-name() = 'team']");
            return teams.Select(t => XmlConvert.Deserialize<Team>(t.ToString()));
        }

        public IEnumerable<Team> Teams(params string[] team_keys)
        {
            return team_keys.Select(Team);
        }
    }
}
