using Dapper;
using FantasyFootball.Core.Objects;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;

namespace FantasyFootball.Terminal.Draft
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
        public IReadOnlyList<Player> PickedPlayers => connection.Query<PlayerDto>(@"
            SELECT Player.*
            FROM DraftPick
            JOIN DraftOption ON DraftPick.DraftOptionId = DraftOption.Id
            JOIN Player ON Player.Id = DraftOption.PlayerId
            WHERE DraftPick.DraftId=@draftId", new { draftId = draftId })
            .Select(FromPlayerDto).ToList();
        public IReadOnlyList<Player> PickedPlayersByParticipant(DraftParticipant t)
        {
            throw new NotImplementedException();
        }
        public IReadOnlyList<Player> UnpickedPlayers => connection.Query<PlayerDto>(@"
            SELECT Player.*
            FROM DraftOption
            JOIN Player ON Player.Id = DraftOption.PlayerId
            LEFT JOIN DraftPick ON DraftPick.DraftOptionId = DraftOption.Id
            WHERE DraftPick.DraftId IS NULL AND DraftOption.DraftId=@draftId", new { draftId = draftId })
            .Select(FromPlayerDto).ToList();

        public class PlayerDto
        {
            public string Id;
            public string Name;
            public string Positions;
            public int TeamId;
        }

        public Player Pick(DraftParticipant t, int r)
        {
            var draftOptionId = connection.QuerySingleOrDefault<string>("SELECT DraftOptionId FROM DraftPick WHERE DraftId=@draftId AND DraftParticipantId=@draftParticipantId AND Round=@round", new
            {
                draftId = draftId,
                draftParticipantId = t.Id,
                round = r
            });

            if (draftOptionId == null) return null;

            var playerId = connection.QuerySingle<string>("SELECT PlayerId FROM DraftOption WHERE Id=@id", new { id = draftOptionId });

            return FromPlayerId(playerId);
        }

        private Player FromPlayerDto(PlayerDto playerDto)
        {
            return new Player
            {
                Id = playerDto.Id,
                Name = playerDto.Name,
                Positions = playerDto.Positions.Split(','),
                Team = connection.QuerySingle<string>("SELECT Name FROM Team WHERE Id=@id", new { id = playerDto.TeamId })
            };
        }

        private Player FromPlayerId(string playerId)
        {
            return connection.Query<PlayerDto>("SELECT * FROM Player WHERE Id=@id", new { id = playerId })
                .Select(FromPlayerDto).Single();
        }

        public void Pick(DraftParticipant t, int r, Player p)
        {
            connection.Execute("INSERT INTO DraftPick (DraftId,DraftOptionId,DraftParticipantId,Round) VALUES (@draftId,@draftOptionId,@draftParticipantId,@round)", new
            {
                draftId = draftId,
                draftOptionId = connection.QuerySingle<string>("SELECT Id FROM DraftOption WHERE DraftId=@draftId AND PlayerId=@playerId", new { draftId = draftId, playerId = p.Id }),
                draftParticipantId = t.Id,
                round = r
            });
        }
    }
}
