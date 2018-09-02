using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using FantasyFootball.Data.Yahoo.Actions;
using FantasyFootball.Data.Yahoo.Models;
using Newtonsoft.Json;
using System.Xml.XPath;
using System;
using Yahoo;

namespace FantasyFootball.Data.Yahoo
{
    public class FantasySportsService
    {
        private readonly FantasySportsWebService webService;

        public FantasySportsService(YahooApiConfig apiConfig)
        {
            webService = new FantasySportsWebService(apiConfig);
        }

        public Game Game(string gameId)
        {
            var json = webService.Game(gameId);
            return JsonConvert.DeserializeObject<WebServiceResponse>(json)?.fantasy_content?.game.Single();
        }

        public StatCategories GameStatCategories(string game_key)
        {
            var xml = webService.GameStatCategories(game_key);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.game?.stat_categories;
        }

        public IEnumerable<Game> Games()
        {
            var filepath = "GameKeys";
            if (!File.Exists(filepath))
                new YearGameIdMapBuilder(filepath, this).Build();
            var ids = File.ReadAllLines(filepath).Select(x => x.Split(':')[1]);
            return Games(ids.ToArray());
        }

        public IEnumerable<Game> Games(params string[] gameIds)
        {
            return gameIds.Select(Game);
        }

        public League League(LeagueKey league_key)
        {
            var xml = webService.League(league_key);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.league;
        }

        public DraftResult[] LeagueDraftResults(LeagueKey league_key)
        {
            var xml = webService.LeagueDraftResults(league_key);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.league?.draft_results;
        }

        public IEnumerable<Player> LeaguePlayers(LeagueKey league_key)
        {
            return ReadAllPlayers(s => webService.LeaguePlayersResults(league_key, s));
        }

        public IEnumerable<Player> LeaguePlayers(LeagueKey league_key, string status)
        {
            return ReadAllPlayers(s => webService.LeaguePlayersResults(league_key, status, s));
        }

        public IEnumerable<Player> LeaguePlayersWeekStats(LeagueKey league_key, int week)
        {
            return ReadAllPlayers(s => webService.LeaguePlayersWeekStats(league_key, week, s));
        }

        private static IEnumerable<Player> ReadAllPlayers(Func<int, string> xmlByStart)
        {
            int start = 0;
            const int size = 25;
            while (true)
            {
                var xml = xmlByStart(start);
                var players = XDocument.Parse(xml).XPathSelectElement("//*[local-name() = 'players']");

                foreach (var player in players.Elements())
                    yield return XmlConvert.Deserialize<Player>(player.ToString());

                var count = players.Attribute("count");
                if (count == null || int.Parse(count.Value) < size)
                    break;

                start += size;
            }
        }

        public LeagueScoreboard LeagueScoreboard(LeagueKey league_key, int week)
        {
            var xml = webService.LeagueScoreboard(league_key, week);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.league?.scoreboard;
        }

        public LeagueSettings LeagueSettings(LeagueKey league_key)
        {
            var xml = webService.LeagueSettings(league_key);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.league?.settings;
        }

        public Transaction[] LeagueTransactions(LeagueKey league_key)
        {
            var xml = webService.LeagueTransactions(league_key);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.league?.transactions;
        }

        public Player Player(string player_key)
        {
            var xml = webService.Player(player_key);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.player;
        }

        public PlayerStats PlayerStats(string player_key, int week)
        {
            var xml = webService.PlayerStats(player_key, week);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.player?.player_stats;
        }

        public IEnumerable<Player> Players(string game_key)
        {
            int start = 0;
            while (true)
            {
                var xml = webService.Players(game_key, start);
                var players = XDocument.Parse(xml).XPathSelectElement("//*[local-name() = 'players']");

                foreach (var player in players.Elements())
                    yield return XmlConvert.Deserialize<Player>(player.ToString());

                var count = players.Attribute("count");
                if (count == null)
                    break;

                start += int.Parse(count.Value);
            }
        }

        public Team Team(string team_key)
        {
            var xml = webService.Team(team_key);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.team;
        }

        public IEnumerable<Team> Teams(LeagueKey league_key)
        {
            var xml = webService.Teams(league_key);
            var doc = XDocument.Parse(xml);
            var teams = doc.XPathSelectElements("//*[local-name() = 'team']");
            return teams.Select(t => XmlConvert.Deserialize<Team>(t.ToString()));
        }

        public Roster TeamRoster(string team_key)
        {
            var xml = webService.TeamRoster(team_key);
            return XmlConvert.Deserialize<FantasyContentXml>(xml)?.team?.roster;
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
