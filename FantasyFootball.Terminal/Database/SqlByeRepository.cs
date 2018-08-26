using Dapper;
using FantasyFootball.Core.Data;
using System.Data.SQLite;

namespace FantasyFootball.Terminal.Database
{
    public class SqlByeRepository : IByeRepository
    {
        private readonly SQLiteConnection connection;

        public SqlByeRepository(SQLiteConnection connection)
        {
            this.connection = connection;
        }

        public int GetByeWeek(int year, string teamName)
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
