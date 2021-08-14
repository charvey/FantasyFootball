using FantasyPros.Projections;
using Hangfire;
using Hangfire.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FantasyPros
{
    public class FantasyProsClient
    {
        private readonly FantasyProsFileClient fileClient;
        private IReadOnlyDictionary<string, FantasyProsPlayerId> playerCache;
        private IReadOnlyDictionary<string, FantasyProsPlayerId> PlayerCache
        {
            get
            {
                if (playerCache == null)
                {
                    var newData = new Dictionary<string, FantasyProsPlayerId>();
                    //TODO remove hardcoded year
                    foreach (var document in fileClient.GetDocuments().Where(x => x.Item1 >= DateTime.Now.AddDays(-4)))
                    {
                        foreach (var player in ProjectionPageParser.ParsePlayers(document.Item2))
                        {
                            if (player.Id == new FantasyProsPlayerId(11872))
                                continue;

                            if (newData.ContainsKey(player.Name) && newData[player.Name] != player.Id)
                                throw new InvalidOperationException();
                            newData[player.Name] = player.Id;
                        }
                    }
                    playerCache = newData;
                }
                return playerCache;
            }
        }
        private IReadOnlyDictionary<FantasyProsPlayerId, IEnumerable<Tuple<DateTime, Projection>>> projectionCache;
        private IReadOnlyDictionary<FantasyProsPlayerId, IEnumerable<Tuple<DateTime, Projection>>> ProjectionCache
        {
            get
            {
                if (projectionCache == null)
                {
                    var newData = new Dictionary<FantasyProsPlayerId, List<Tuple<DateTime, Projection>>>();
                    foreach (var document in fileClient.GetDocuments())
                    {
                        foreach (var projection in ProjectionPageParser.ParseProjections(document.Item2))
                        {
                            if (!newData.ContainsKey(projection.Item1))
                                newData[projection.Item1] = new List<Tuple<DateTime, Projection>>();
                            newData[projection.Item1].Add(Tuple.Create(document.Item1, projection.Item2));
                        }
                    }
                    projectionCache = newData.ToDictionary(x => x.Key, x => x.Value.AsEnumerable());
                }
                return projectionCache;
            }
        }

        public FantasyProsClient(string dataDirectory, IRecurringJobManager recurringJobManager)
        {
            this.fileClient = new FantasyProsFileClient(dataDirectory);
            recurringJobManager.AddOrUpdate("FantasyPros Update", Job.FromExpression<FantasyProsClient>(client => client.Update()), Cron.MinuteInterval(15));
            this.Update();
        }

        public DstProjection GetDstProjection(FantasyProsPlayerId playerId, DateTime at) => (DstProjection)GetProjection(playerId, at);
        public KProjection GetKProjection(FantasyProsPlayerId playerId, DateTime at) => (KProjection)GetProjection(playerId, at);
        public QbProjection GetQbProjection(FantasyProsPlayerId playerId, DateTime at) => (QbProjection)GetProjection(playerId, at);
        public RbProjection GetRbProjection(FantasyProsPlayerId playerId, DateTime at) => (RbProjection)GetProjection(playerId, at);
        public TeProjection GetTeProjection(FantasyProsPlayerId playerId, DateTime at) => (TeProjection)GetProjection(playerId, at);
        public WrProjection GetWrProjection(FantasyProsPlayerId playerId, DateTime at) => (WrProjection)GetProjection(playerId, at);

        private Projection GetProjection(FantasyProsPlayerId playerId, DateTime at)
        {
            return ProjectionCache[playerId].Where(x => x.Item1 <= at && x.Item1 >= at.AddDays(-3)).OrderByDescending(x => x.Item1).First().Item2;
        }

        [Obsolete]
        public FantasyProsPlayerId? TempGetPlayerId(string name)
        {
            var nameToSearchFor = NameToSearchFor(name);
            if (PlayerCache.ContainsKey(nameToSearchFor))
                return PlayerCache[nameToSearchFor];
            return null;
        }

        private static readonly string[] endings = new[] { "II", "III", "IV", "V", "Jr.", "Sr." };
        private static string NameToSearchFor(string playerName)
        {
            if (playerName == "Chris Thompson") return "COLLIDES";
            if (playerName == "Ryan Griffin") return "COLLIDES";
            if (playerName == "Tavon Austin") return "DOUBLE LISTED";
            if (playerName == "Malcolm Perry") return "DOUBLE LISTED";
            if (playerName == "Taysom Hill") return "DOUBLE LISTED";
            if (playerName == "Stephen Sullivan") return "DOUBLE LISTED";
            if (playerName == "Reggie Gilliam") return "DOUBLE LISTED";
            if (playerName == "Dylan Cantrell") return "DOUBLE LISTED";
            if (playerName == "Jakob Johnson") return "DOUBLE LISTED";
            if (playerName == "Cordarrelle Patterson") return "DOUBLE LISTED";
            if (playerName == "John Lovett") return "DOUBLE LISTED";

            if (playerName == "Odell Beckham Jr.") return playerName;
            if (playerName == "Simmie Cobbs Jr.") return playerName;
            if (playerName == "Ronald Jones II") return playerName;
            if (playerName == "Gardner Minshew II") return playerName;
            if (playerName == "A.J. Brown") return playerName;
            if (playerName == "T.Y. Hilton") return playerName;
            if (playerName == "O.J. Howard") return playerName;
            if (playerName == "D.J. Moore") return playerName;
            if (playerName == "J.K. Dobbins") return playerName;
            if (playerName == "C.J. Beathard") return playerName;
            if (playerName == "Equanimeous St. Brown") return playerName;
            if (playerName == "A.J. Green") return playerName;
            if (playerName == "C.J. Uzomah") return playerName;
            if (playerName == "Willie Snead IV") return playerName;
            if (playerName == "DK Metcalf") return "D.K. Metcalf";

            foreach (var ending in endings)
            {
                if (playerName.EndsWith(" " + ending))
                    playerName = playerName.Remove(playerName.Length - (ending.Length + 1));
            }

            playerName = playerName.Replace(".", "");

            if (playerName == "Mitchell Trubisky") return "Mitch Trubisky";
            else if (playerName == "Patrick Mahomes") return "Patrick Mahomes II";
            else if (playerName == "Rob Kelley") return "Robert Kelley";
            else if (playerName == "Stephen Hauschka") return "Steven Hauschka";
            return playerName;
        }

        public double GetADP(string playerName)
        {
            throw new NotImplementedException();
        }

        public void Update()
        {
            if (fileClient.Scrape())
            {
                playerCache = null;
                projectionCache = null;
            }
        }
    }
}
