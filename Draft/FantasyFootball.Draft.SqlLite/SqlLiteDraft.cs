using Dapper;
using FantasyFootball.Draft.Abstractions;
using System.Data.SQLite;

namespace FantasyFootball.Draft.SqlLite
{
    internal class SqlLiteDraft : IDraft
    {
        private readonly SQLiteConnection connection;
        private readonly string draftId;

        public SqlLiteDraft(SQLiteConnection connection, string draftId)
        {
            this.connection = connection;
            this.draftId = draftId;
        }

        public IReadOnlyList<DraftParticipant> Participants => connection.Query<DraftParticipant>("SELECT * FROM DraftParticipant WHERE DraftId=@draftId ORDER BY [Order]", new { draftId }).ToList();
        public DraftParticipant ParticipantByPlayer(Player player) =>
            connection.Query<DraftParticipant>(@"
                SELECT DraftParticipant.*
                FROM DraftParticipant
                JOIN DraftPick ON DraftPick.DraftParticipantId=DraftParticipant.Id
                JOIN DraftOption ON DraftPick.DraftOptionId = DraftOption.Id
                WHERE DraftOption.DraftId=@draftId AND DraftOption.PlayerId=@playerId",
                new { draftId, playerId = player.Id })
            .SingleOrDefault();

        public IReadOnlyList<Player> AllPlayers => connection.Query<PlayerDto>(@"
            SELECT Player.*
            FROM DraftOption
            JOIN Player ON Player.Id = DraftOption.PlayerId
            WHERE DraftOption.DraftId=@draftId",
            new { draftId })
            .Select(FromPlayerDto).ToList();
        public IReadOnlyList<Player> PickedPlayers => connection.Query<PlayerDto>(@"
            SELECT Player.*
            FROM DraftPick
            JOIN DraftOption ON DraftPick.DraftOptionId = DraftOption.Id
            JOIN Player ON Player.Id = DraftOption.PlayerId
            WHERE DraftPick.DraftId=@draftId",
            new { draftId })
            .Select(FromPlayerDto).ToList();
        public IReadOnlyList<Player> PickedPlayersByParticipant(DraftParticipant t) => connection.Query<PlayerDto>(@"
            SELECT Player.*
            FROM DraftPick
            JOIN DraftOption ON DraftPick.DraftOptionId = DraftOption.Id
            JOIN Player ON Player.Id = DraftOption.PlayerId
            WHERE DraftPick.DraftId=@draftId AND DraftPick.DraftParticipantId=@draftParticipantId",
            new { draftId, draftParticipantId = t.Id })
            .Select(FromPlayerDto).ToList();
        public IReadOnlyList<Player> UnpickedPlayers => connection.Query<PlayerDto>(@"
            SELECT Player.*
            FROM DraftOption
            JOIN Player ON Player.Id = DraftOption.PlayerId
            LEFT JOIN DraftPick ON DraftPick.DraftOptionId = DraftOption.Id
            WHERE DraftPick.DraftId IS NULL AND DraftOption.DraftId=@draftId",
            new { draftId })
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
                draftId,
                draftParticipantId = t.Id,
                round = r
            });

            if (draftOptionId == null) return null;

            return connection.Query<PlayerDto>(@"
                SELECT Player.*
                FROM DraftOption
                JOIN Player ON Player.Id = DraftOption.PlayerId
                WHERE DraftOption.Id=@id AND DraftOption.DraftId=@draftId",
            new { draftId, id = draftOptionId })
                .Select(FromPlayerDto).Single();
        }

        private Player FromPlayerDto(PlayerDto playerDto)
        {
            return new Player
            (
                Id: playerDto.Id,
                Name: playerDto.Name,
                Positions: playerDto.Positions.Split(','),
                Team: connection.QuerySingle<string>("SELECT Name FROM Team WHERE Id=@id", new { id = playerDto.TeamId })
            );
        }

        public void Pick(DraftParticipant t, int r, Player p)
        {
            connection.Execute("INSERT INTO DraftPick (DraftId,DraftOptionId,DraftParticipantId,Round) VALUES (@draftId,@draftOptionId,@draftParticipantId,@round)", new
            {
                draftId,
                draftOptionId = connection.QuerySingle<string>("SELECT Id FROM DraftOption WHERE DraftId=@draftId AND PlayerId=@playerId", new { draftId, playerId = p.Id }),
                draftParticipantId = t.Id,
                round = r
            });
        }
    }
}
