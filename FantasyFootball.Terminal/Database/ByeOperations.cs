using Dapper;
using System.Data.SQLite;

namespace FantasyFootball.Terminal.Database
{
    public static class ByeOperations
    {
        public static int GetByeWeek(this SQLiteConnection connection, int year, string teamName)
        {
            return connection.QuerySingle<int>(@"
                SELECT Week
                FROM Bye
                JOIN Team ON Bye.TeamId=Team.Id
                WHERE Bye.Year=@year AND Team.Name=@name", new
            {
                year = year,
                name = teamName
            });
        }
    }
}
