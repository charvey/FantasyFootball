using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Modules
{
    public class GameHistory : Module
    {
        protected override List<string> Dependencies
        {
            get { return new List<string> { "PFR" }; }
        }

        protected override void Initialize()
        {
            PFR pfr = DependencyModules["PFR"] as PFR;

            for (int year = 1999; year <= 2013; year++)
            {
                HtmlDocument doc = new HtmlDocument();
                doc.Load(File.OpenRead(pfr.GetPath(string.Format("years/{0}/games.htm", year))));
                var nodes = doc.DocumentNode.SelectNodes("//table[@id='games']/tbody/tr/td[4]/a");

                List<string> games = new List<string>();
                if (nodes != null)
                {
                    foreach (var node in nodes)
                    {
                        var gameId = node.Attributes["href"].Value;
                        gameId = gameId.Substring(gameId.LastIndexOf('/') + 1, 12);

                        games.Add(gameId);
                    }
                }
                Games[year] = games;
            }
        }

        public Dictionary<int, List<string>> Games = new Dictionary<int, List<string>>();
        public IEnumerable<int> Years
        {
            get
            {
                return Games.Keys.OrderBy(y => y);
            }
        }
    }
}
