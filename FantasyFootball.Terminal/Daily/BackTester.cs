using YahooDailyFantasy;

namespace FantasyFootball.Terminal.Daily
{
    public class BackTester
    {
        public static void Do(YahooDailyFantasyClient yahooDailyFantasyClient, string dataDirectory)
        {
            foreach (var contest in yahooDailyFantasyClient.MyContests(dataDirectory).Where(c => !c.isCanceled))
            {
                Console.WriteLine($"{contest.id} {contest.title} {contest.winnings:C} {contest.rank}/{contest.entryCount}");
            }
        }
    }
}
