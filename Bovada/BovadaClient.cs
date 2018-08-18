using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;

namespace Bovada
{
    public class Path
    {
        public string id { get; set; }
        public string link { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string sportCode { get; set; }
        public int order { get; set; }
        public bool leaf { get; set; }
        public bool current { get; set; }
    }

    public class Competitor
    {
        public string id { get; set; }
        public string name { get; set; }
        public bool home { get; set; }
    }

    public class Period
    {
        public string id { get; set; }
        public string description { get; set; }
        public string abbreviation { get; set; }
        public bool live { get; set; }
        public bool main { get; set; }
    }

    public class Price
    {
        public string id { get; set; }
        public string handicap { get; set; }
        public string american { get; set; }
        public string @decimal { get; set; }
        public string fractional { get; set; }
    }

    public class Outcome
    {
        public string id { get; set; }
        public string description { get; set; }
        public string status { get; set; }
        public string type { get; set; }
        public string competitorId { get; set; }
        public Price price { get; set; }
    }

    public class Market
    {
        public string id { get; set; }
        public string description { get; set; }
        public string key { get; set; }
        public string marketTypeId { get; set; }
        public string status { get; set; }
        public bool singleOnly { get; set; }
        public string notes { get; set; }
        public Period period { get; set; }
        public List<Outcome> outcomes { get; set; }
    }

    public class DisplayGroup
    {
        public string id { get; set; }
        public string description { get; set; }
        public bool defaultType { get; set; }
        public bool alternateType { get; set; }
        public List<Market> markets { get; set; }
    }

    public class Event
    {
        public string id { get; set; }
        public string description { get; set; }
        public string type { get; set; }
        public string link { get; set; }
        public string status { get; set; }
        public string sport { get; set; }
        public object startTime { get; set; }
        public bool live { get; set; }
        public bool awayTeamFirst { get; set; }
        public bool denySameGame { get; set; }
        public bool teaserAllowed { get; set; }
        public string competitionId { get; set; }
        public string notes { get; set; }
        public int numMarkets { get; set; }
        public object lastModified { get; set; }
        public List<Competitor> competitors { get; set; }
        public List<DisplayGroup> displayGroups { get; set; }
    }

    public class RootObject
    {
        public List<Path> path { get; set; }
        public List<Event> events { get; set; }
    }

    public class BovadaClient
    {
        private readonly WebClient webClient = new WebClient();

        public IReadOnlyList<Event> NflPreseason()
        {
            var json = webClient.DownloadString("https://www.bovada.lv/services/sports/event/v2/events/A/description/football/nfl-preseason");
            json = json.TrimStart('[').TrimEnd(']');
            return JsonConvert.DeserializeObject<RootObject>(json).events;
        }
    }
}
