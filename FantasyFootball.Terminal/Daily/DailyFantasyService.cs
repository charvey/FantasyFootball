using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace FantasyFootball.Terminal.Daily
{
    public static class DailyFantasyService
    {
        private static HttpClient httpClient = new HttpClient();

        public static List<DailyPlayer> GetPlayers(int contestId)
        {
            var playerUrl = $"https://dfyql-ro.sports.yahoo.com/v2/export/contestPlayers?contestId={contestId}";
            var lines = httpClient.GetStringAsync(playerUrl).Result.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Skip(1);

            return lines
                .Select(l => l.Split(',')).Select(l => new DailyPlayer
                {
                    Id = l[0],
                    Name = l[1] + " " + l[2],
                    Position = l[3],
                    Salary = int.Parse(l[8])
                }).ToList();
        }

        public static Contest GetContest(int contestId)
        {
            var json = httpClient.GetStringAsync($"https://dfyql-ro.sports.yahoo.com/v2/contest/{contestId}").Result;
            return JsonConvert.DeserializeObject<Response>(json).contests.result.Single();
        }
    }

    public class Response
    {
        public Contests contests { get; set; }
    }

    public class Contests
    {
        public Contest[] result { get; set; }
        public object error { get; set; }
    }

    public class Contest
    {
        public int id { get; set; }
        public int salaryCap { get; set; }
        public int seriesId { get; set; }
        public long startTime { get; set; }
    }
}
