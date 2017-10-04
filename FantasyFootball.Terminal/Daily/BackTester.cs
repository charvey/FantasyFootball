using System;
using System.Linq;

namespace FantasyFootball.Terminal.Daily
{
    public class BackTester
    {
        public static void Do(string dataDirectory)
        {
            foreach (var contest in DailyFantasyService.MyContests(dataDirectory).Where(c => !c.isCanceled))
            {
                Console.WriteLine($"{contest.id} {contest.title} {contest.winnings:C} {contest.rank}/{contest.entryCount}");
            }
        }
    }
}
