using FantasyFootball.Service.Fantasy.Models;
using Microsoft.Data.Entity;

namespace FantasyFootball.Service.Fantasy
{
    public class FantasyContext : DbContext
    {
        public DbSet<League> Leagues { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Team> Teams { get; set; }

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
