using Dapper;
using FantasyFootball.Core.Objects;
using System.Data.SQLite;
using System.Linq;

namespace FantasyFootball.Terminal.Database
{
    public static class PlayerOpertations
    {
        public class PlayerDto
        {
            public string Id;
            public string Name;
            public string Positions;
            public int TeamId;
        }

        private static Player FromPlayerDto(SQLiteConnection connection, PlayerDto playerDto)
        {
            return new Player
            {
                Id = playerDto.Id,
                Name = playerDto.Name,
                Positions = playerDto.Positions.Split(','),
                Team = connection.QuerySingle<string>("SELECT Name FROM Team WHERE Id=@id", new { id = playerDto.TeamId })
            };
        }

        public static Player GetPlayer(this SQLiteConnection connection, string playerId)
        {
            return connection.Query<PlayerDto>("SELECT * FROM Player WHERE Id=@id", new { id = playerId })
                .Select(p => FromPlayerDto(connection, p)).Single();
        }
    }
}
