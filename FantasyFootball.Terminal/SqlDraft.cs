using Dapper;
using FantasyFootball.Core.Draft;
using FantasyFootball.Core.Objects;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

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

        public IReadOnlyList<DraftParticipant> Participants => connection.Query<DraftParticipant>("SELECT * FROM DraftParticipant WHERE DraftId=@draftId", new { draftId = draftId }).ToList();

        public IReadOnlyList<Player> AllPlayers => throw new NotImplementedException();
        public IReadOnlyList<Player> PickedPlayers => throw new NotImplementedException();
        public IReadOnlyList<Player> PickedPlayersByParticipant(DraftParticipant t)
        {
            throw new NotImplementedException();
        }
        public IReadOnlyList<Player> UnpickedPlayers => throw new NotImplementedException();

        public class PlayerDto
        {
            public string Id;
            public string Name;
            public string Positions;
            public int TeamId;
        }

        public Player Pick(DraftParticipant t, int r)
        {
            var draftOptionId = connection.QuerySingle<string>("SELECT DraftOptionId FROM DraftPick WHERE DraftId=@draftId AND DraftParticipantId=@draftParticipantId AND Round=@round", new
            {
                draftId = draftId,
                draftParticipantId = t.Id,
                round = r
            });

            var playerId = connection.QuerySingle<string>("SELECT PlayerId FROM DraftOption WHERE Id=@id", new { id = draftOptionId });

            return connection.Query<PlayerDto>("SELECT * FROM Player WHERE Id=@id", new { id = playerId })
                .Select(p => new Player
                {
                    Id = p.Id,
                    Name = p.Name,
                    Positions = p.Positions.Split(','),
                    Team = connection.QuerySingle<string>("SELECT Name FROM Team WHERE Id=@id", new { id = p.TeamId })
                }).Single();
        }

        public void Pick(DraftParticipant t, int r, Player p)
        {
            throw new NotImplementedException();
        }
    }
}
