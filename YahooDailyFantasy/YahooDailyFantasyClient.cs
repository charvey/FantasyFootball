using Newtonsoft.Json;

namespace YahooDailyFantasy
{
    public class YahooDailyPlayer
    {
        public string Id;
        public string FirstName;
        public string LastName;
        public string Position;
        public int Salary;
    }

    public class YahooDailyFantasyClient
    {
        private HttpClient httpClient = new HttpClient();

        public List<YahooDailyPlayer> GetPlayers(int contestId)
        {
            var playerUrl = $"https://dfyql-ro.sports.yahoo.com/v2/export/contestPlayers?contestId={contestId}";
            var lines = httpClient.GetStringAsync(playerUrl).Result.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).Skip(1);

            return lines
                .Select(l => l.Split(',')).Select(l => new YahooDailyPlayer
                {
                    Id = l[0],
                    FirstName = l[1],
                    LastName = l[2],
                    Position = l[3],
                    Salary = int.Parse(l[8])
                }).ToList();
        }

        public Contest GetContest(int contestId)
        {
            var json = httpClient.GetStringAsync($"https://dfyql-ro.sports.yahoo.com/v2/contest/{contestId}").Result;
            return JsonConvert.DeserializeObject<ContestResponse>(json).contests.result.Single();
        }

        public IEnumerable<UserContest> MyContests(string dataDirectory)
        {
            var json = File.ReadAllText(Path.Combine(dataDirectory, "userContests.json"));
            return JsonConvert.DeserializeObject<UserContestResponse>(json).contests.result;
        }
    }

    public class ContestResponse
    {
        public Contests contests { get; set; }
        public long currentTime { get; set; }
        public Pagination pagination { get; set; }
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

    public class UserContestResponse
    {
        public UserContests contests { get; set; }
        public long currentTime { get; set; }
        public Pagination pagination { get; set; }
    }

    public class UserContests
    {
        public UserContest[] result { get; set; }
        public object error { get; set; }
    }

    public class UserContest
    {
        public int contestEntryId { get; set; }
        public int rank { get; set; }
        public double? percentile { get; set; }
        public double score { get; set; }
        public double winnings { get; set; }
        public Money paidWinnings { get; set; }
        public object prize { get; set; }
        public string currency { get; set; }
        public bool isCanceled { get; set; }
        public string status { get; set; }
        public double entryFee { get; set; }
        public Money paidEntryFee { get; set; }
        public int id { get; set; }
        public string title { get; set; }
        public string type { get; set; }
        public string sportCode { get; set; }
        public int seriesId { get; set; }
        public int entryCount { get; set; }
        public double totalPrize { get; set; }
        public Money paidTotalPrize { get; set; }
        public int entryLimit { get; set; }
        public int multipleEntryLimit { get; set; }
        public bool multipleEntry { get; set; }
        public int salaryCap { get; set; }
        public long startTime { get; set; }
        public double topPrize { get; set; }
        public string state { get; set; }
        public string scope { get; set; }
        public bool guaranteed { get; set; }
        public string subleague { get; set; }
        public string subleagueDisplayName { get; set; }
        public object opponentExperience { get; set; }
        public string restriction { get; set; }
    }

    public class PaginationResult
    {
        public int start { get; set; }
        public int limit { get; set; }
        public int totalCount { get; set; }
    }

    public class Pagination
    {
        public PaginationResult result { get; set; }
        public object error { get; set; }
    }

    public class Money
    {
        public double value { get; set; }
        public string currency { get; set; }
        public double amount { get; set; }
    }
}
