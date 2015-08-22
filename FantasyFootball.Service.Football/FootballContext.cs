using FantasyFootball.Service.Football.Models;
using Microsoft.Data.Entity;

namespace FantasyFootball.Service.Football
{
    public class FootballContext : DbContext
    {
        public DbSet<Player> Players { get; set; }

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
