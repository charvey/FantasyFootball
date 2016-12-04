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
            var filepath = "GameKeys";
            if (!File.Exists(filepath))
                new YearGameIdMapBuilder(filepath).Build();
            var ids = File.ReadAllLines(filepath).Select(x => x.Split(':')[1]);
            return Games(ids.ToArray());
        }

        public IEnumerable<Game> Games(params string[] gameIds)
        {
            return gameIds.Select(Game);
        }

        public League League(string league_key)
        {
            var xml = webService.League(league_key);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.league;
        }

        public DraftResult[] LeagueDraftResults(string league_key)
        {
            var xml = webService.LeagueDraftResults(league_key);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.league?.draft_results;
        }

        public LeagueScoreboard LeagueScoreboard(string league_key, int week)
        {
            var xml = webService.LeagueScoreboard(league_key, week);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.league?.scoreboard;
        }

        public Transaction[] LeagueTransactions(string league_key)
        {
            var xml = webService.LeagueTransactions(league_key);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.league?.transactions;
        }

        public IEnumerable<League> Leagues(params string[] league_keys)
        {
            return league_keys.Select(League);
        }

        public Player Player(string player_key)
        {
            var xml = webService.Player(player_key);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.player;
        }

        public IEnumerable<Player> Players(string game_key)
        {
            int start = 0;
            const int size = 25;
            while (true)
            {
                var xml = webService.Players(game_key, start);
                var players = XDocument.Parse(xml).XPathSelectElement("//*[local-name() = 'players']");

                foreach (var player in players.Elements())
                    yield return XmlConvert.Deserialize<Player>(player.ToString());

                var count = players.Attribute("count");
                if (count == null || int.Parse(count.Value) < size)
                    break;

                start += size;
            }
        }

        public Team Team(string team_key)
        {
            var xml = webService.Team(team_key);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.team;
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

        public Roster TeamRoster(string team_key, int week)
        {
            var xml = webService.TeamRoster(team_key, week);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.team?.roster;
        }

        public IEnumerable<Team> Teams(params string[] team_keys)
        {
            return team_keys.Select(Team);
        }
    }
}
