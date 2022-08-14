using Dapper;
using FantasyFootball.Draft.Abstractions;
using System.Data.SQLite;

namespace FantasyFootball.Draft.SqlLite
{
    public class SqlLiteDraftProvider : IDraftProvider
    {
        private readonly SQLiteConnection connection;

        public SqlLiteDraftProvider(SQLiteConnection connection)
        {
            this.connection = connection;
        }

        public IReadOnlyList<IDraftProvider.DraftEntry> GetDrafts()
        {
            return connection.Query<string>("SELECT Id FROM Draft")
                .Select(id => new IDraftProvider.DraftEntry(id, () => new SqlLiteDraft(connection, id)))
                .ToList();
        }
    }
}
