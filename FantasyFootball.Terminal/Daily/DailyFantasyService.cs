using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;

namespace FantasyFootball.Terminal.Daily
{
    public static class DailyFantasyService
    {
        public static List<DailyPlayer> GetPlayers(int contestId)
        {
            var playerUrl = $"https://dfyql-ro.sports.yahoo.com/v2/export/contestPlayers?contestId={contestId}";
            var lines = new HttpClient().GetStringAsync(playerUrl).Result.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Skip(1);

            return lines
                .Select(l => l.Split(',')).Select(l => new DailyPlayer
                {
                    Id = l[0],
                    Name = l[1] + " " + l[2],
                    Position = l[3],
                    Salary = int.Parse(l[8])
                }).ToList();
        }
    }
}
