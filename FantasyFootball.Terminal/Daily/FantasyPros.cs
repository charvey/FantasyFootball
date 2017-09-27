using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FantasyFootball.Terminal.Daily
{
    public class FantasyPros
    {
        private readonly Dictionary<string, HtmlDocument> positionPage;
        private readonly string dataDirectory;

        public FantasyPros(string dataDirectory)
        {
            this.positionPage = new Dictionary<string, HtmlDocument>();
            this.dataDirectory = dataDirectory;
        }

        private HtmlDocument DocumentByPosition(string position)
        {
            if (!positionPage.ContainsKey(position))
            {
                string filename;
                switch (position)
                {
                    case "QB": filename = @"qb.html"; break;
                    case "WR": filename = @"wr.html"; break;
                    case "RB": filename = @"rb.html"; break;
                    case "TE": filename = @"te.html"; break;
                    case "DEF": filename = @"dst.html"; break;
                    default: throw new ArgumentOutOfRangeException();
                }
                var file = Directory.EnumerateDirectories(dataDirectory + @"\fantasypros")
                    .OrderByDescending(d => DateTime.ParseExact(Path.GetFileName(d), "yyyy-MM-dd HH-mm-ss", (IFormatProvider)null))
                    .Select(d => Path.Combine(d, filename))
                    .First(f => File.Exists(f));
                var document = new HtmlDocument();
                document.LoadHtml(File.ReadAllText(file));
                positionPage[position] = document;
            }
            return positionPage[position];
        }


        static string NameToSearchFor(DailyPlayer player)
        {
            if (player.Name == "Todd Gurley II") return "Todd Gurley";
            else if (player.Name == "Terrelle Pryor Sr.") return "Terrelle Pryor";
            else if (player.Name == "Patrick Mahomes II") return "Patrick Mahomes";
            else if (player.Name == "Mitchell Trubisky") return "Mitch Trubisky";
            else if (player.Name == "Rob Kelley") return "Robert Kelley";
            return player.Name;
        }

        public HtmlNode GetPlayerRow(DailyPlayer player)
        {
            var table = DocumentByPosition(player.Position).GetElementbyId("data");
            var nameToSearchFor = NameToSearchFor(player);
            return table.Element("tbody").Elements("tr").SingleOrDefault(tr => tr.Elements("td").First().Elements("a").First().InnerText == nameToSearchFor);
        }
    }
}
