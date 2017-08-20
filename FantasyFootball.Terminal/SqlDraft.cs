using FantasyFootball.Core.Draft;
using FantasyFootball.Core.Objects;
using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace FantasyFootball.Terminal
{
    public class SqlDraft : Draft
    {
        private readonly SQLiteConnection connection;
        private readonly string draftId;

        public SqlDraft(SQLiteConnection connection, string draftId)
        {
            this.connection = connection;
            this.draftId = draftId;
        }

        public IReadOnlyList<Team> Teams => throw new NotImplementedException();

        public IReadOnlyList<Player> PickedPlayers => throw new NotImplementedException();

        public Player Pick(Team t, int r)
        {
            throw new NotImplementedException();
        }

        public void Pick(Team t, int r, Player p)
        {
            throw new NotImplementedException();
        }

        public IReadOnlyList<Player> PickedPlayersByTeam(Team t)
        {
            throw new NotImplementedException();
        }
    }
}
