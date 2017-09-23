using HtmlAgilityPack;
using System;
using System.IO;
using System.Linq;
using System.Net;

namespace FantasyFootball.Terminal.Scraping
{
    class FantasyPros
    {
        private const string dateFormat = "yyyy-MM-dd HH-mm-ss";

        public static void Scrape(string dataDirectory)
        {
            var client = new WebClient();
            foreach (var pos in new[] { "qb", "rb", "wr", "te", "k", "dst" })
            {
                var html = client.DownloadString($"https://www.fantasypros.com/nfl/projections/{pos}.php?scoring=HALF");
                var document = new HtmlDocument();
                document.LoadHtml(html);
                var time = document.DocumentNode.Descendants("time").Single();
                var datetime = DateTime.Parse(time.GetAttributeValue("datetime", ""));
                var folder = $@"{dataDirectory}\fantasypros\{datetime.ToString(dateFormat)}";
                Directory.CreateDirectory(folder);
                File.WriteAllText($@"{folder}\{pos}.html", html);
            }
        }
    }
}
