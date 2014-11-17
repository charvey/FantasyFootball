using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Terminal.Modules
{
    public class Boxscores : Module
    {
        private GameHistory GameHistory
        {
            get
            {
                return DependencyModules["GameHistory"] as GameHistory;
            }
        }

        private PFR pfr;

        protected override void Initialize()
        {
            pfr = DependencyModules["PFR"] as PFR;

            foreach (int year in GameHistory.Years)
            {
                foreach (string gameId in GameHistory.Games[year])
                {
                    pfr.GetPath("boxscores/" + gameId + ".htm");
                }
            }
        }

        public IEnumerable<string> GameIds
        {
            get
            {
                return Directory.EnumerateFiles("pfr/boxscores").Select(p => Path.GetFileNameWithoutExtension(p));
            }
        }

        public HtmlDocument GetPage(string GameId)
        {
            HtmlDocument document = new HtmlDocument();
            document.Load(File.OpenRead(pfr.GetPath("boxscores/" + GameId + ".htm")));
            return document;
        }

        protected override List<string> Dependencies
        {
            get { return new List<string> { "PFR", "GameHistory" }; }
        }
    }
}
