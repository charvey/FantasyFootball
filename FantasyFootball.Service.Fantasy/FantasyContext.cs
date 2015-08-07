using Microsoft.Data.Entity;

namespace FantasyFootball.Service.Fantasy
{
    public class FantasyContext : DbContext
    {
        public DbSet<League> Leagues { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder builder)
        {
            builder.UseSqlite(@"Data Source=C:\_Projects\FantasyFootball\Data\Fantasy.db;");
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<League>().Key(l => l.Id);
            builder.Entity<Team>().Key(t => t.Id);
            base.OnModelCreating(builder);
        }
    }
}
