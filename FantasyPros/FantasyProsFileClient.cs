using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;

namespace FantasyPros
{
    internal class FantasyProsFileClient
    {
        private const string dateFormat = "yyyy-MM-dd HH-mm-ss";
        private readonly string dataDirectory;

        public FantasyProsFileClient(string dataDirectory)
        {
            this.dataDirectory = dataDirectory;
        }

        public bool Scrape()
        {
            var newFiles = false;
            var client = new WebClient();
            foreach (var pos in new[] { "qb", "rb", "wr", "te", "k", "dst" })
            {
                var html = client.DownloadString($"https://www.fantasypros.com/nfl/projections/{pos}.php?scoring=HALF");
                var document = new HtmlDocument();
                document.LoadHtml(html);
                var datetime = ProjectionPageParser.ParseTime(document);
                var filename = Path.Combine(dataDirectory, "fantasypros", datetime.ToString(dateFormat), $"{pos}.html");
                if (!File.Exists(filename))
                {
                    newFiles = true;
                    Directory.CreateDirectory(Path.GetDirectoryName(filename));
                    File.WriteAllText(filename, html);
                }
            }
            return newFiles;
        }

        public IEnumerable<Tuple<DateTime, HtmlDocument>> GetDocuments()
        {
            return Directory.EnumerateDirectories(dataDirectory + @"\fantasypros")
                      .SelectMany(d => Directory.EnumerateFiles(d)
                          .Select(f => Tuple.Create(DateTime.ParseExact(Path.GetFileName(d), dateFormat, (IFormatProvider)null), HtmlDocumentFromFile(f)))
                      );
        }

        private static HtmlDocument HtmlDocumentFromFile(string filename)
        {
            var document = new HtmlDocument();
            document.LoadHtml(File.ReadAllText(filename));
            return document;
        }
    }
}
