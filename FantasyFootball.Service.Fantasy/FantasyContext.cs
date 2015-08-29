using FantasyFootball.Service.Fantasy.Models;
using Microsoft.Data.Entity;

namespace FantasyFootball.Service.Fantasy
{
    public class FantasyContext : DbContext
    {
        public DbSet<League> Leagues { get; set; }
        public DbSet<LeaguePlayer> LeaguePlayers { get; set; }
        public DbSet<PlayerPosition> PlayerPosition { get; set; }
        public DbSet<Team> Teams { get; set; }
        public DbSet<RosterPosition> RosterPositions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlite(ConnectionString.DataSource);
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
