using Dapper;
using FantasyFootball.Core.Data;
using FantasyFootball.Core.Objects;
using System.Data.SQLite;
using System.Linq;

namespace FantasyFootball.Terminal.Database
{
    public class SqlPlayerRepository : IPlayerRepository
    {
        private SQLiteConnection connection;

        private class PlayerDto
        {
            public string Id;
            public string Name;
            public string Positions;
            public int TeamId;
        }

        public SqlPlayerRepository(SQLiteConnection connection)
        {
            this.connection = connection;
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

        public Player GetPlayer(string playerId)
        {
            return connection.Query<PlayerDto>("SELECT * FROM Player WHERE Id=@id", new { id = playerId })
                .Select(p => FromPlayerDto(p)).Single();
        }
    }
}
