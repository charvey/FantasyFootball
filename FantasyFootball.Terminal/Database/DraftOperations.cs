﻿using Dapper;
using System.Data.SQLite;
using System.Linq;

namespace FantasyFootball.Terminal.Database
{
    public static class DraftOperations
    {
        public static void DeleteDraft(this SQLiteConnection connection, string draftId)
        {
            using (var transaction = connection.BeginTransaction())
            {
                connection.Execute("DELETE FROM DraftPick WHERE DraftId=@id", new { id = draftId });
                connection.Execute("DELETE FROM DraftOption WHERE DraftId=@id", new { id = draftId });
                connection.Execute("DELETE FROM DraftParticipant WHERE DraftId=@id", new { id = draftId });
                connection.Execute("DELETE FROM Draft WHERE Id=@id", new { id = draftId });
                transaction.Commit();
            }
        }

        public static string[] GetDraftIds(this SQLiteConnection connection)
        {
            return connection.Query<string>("SELECT Id FROM Draft").ToArray();
        }
    }
}
