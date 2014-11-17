using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Terminal
{
    public class PFR:Module
    {
        private const string ROOT_URL = "http://www.pro-football-reference.com/";
        private string LOCAL_PATH = "PFR";
        private Object locker=new Object();
        private DateTime WaitFor = DateTime.Now;
        private Random Random = new Random();

        protected override List<string> Dependencies
        {
            get { return new List<string> { }; }
        }

        protected override void Initialize()
        {
            Directory.CreateDirectory("PFR");
        }

        public string GetPath(string path)
        {
            var rpath = ROOT_URL + path;
            var lpath = Path.Combine(LOCAL_PATH, path);

            bool needed = !File.Exists(lpath);
            if (!needed)
            {
                TimeSpan expirationAge = TimeSpan.FromDays(120);
                TimeSpan distributionRange = TimeSpan.FromDays(60);
                FileInfo info = new FileInfo(lpath);
                DateTime expiration = info.LastWriteTime.Add(expirationAge);
                TimeSpan distributed = TimeSpan.FromHours(info.Length % distributionRange.TotalHours);
                expiration = expiration.Add(distributed);
                needed = DateTime.Now > expiration;
            }

            if (needed)
            {
                WebClient client = new WebClient();
                Directory.CreateDirectory(Path.GetDirectoryName(lpath));
                lock (locker)
                {
                    DateTime now = DateTime.Now;
                    if (WaitFor > now)
                    {
                        Thread.Sleep(WaitFor.Subtract(now));
                    }
                    client.DownloadFile(rpath, lpath);
                    WaitFor = DateTime.Now.Add(TimeSpan.FromSeconds(Random.Next(10, 30)));
                }
            }

            return lpath;
        }
    }
}
