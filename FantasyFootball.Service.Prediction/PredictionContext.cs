using Microsoft.Data.Entity;

namespace FantasyFootball.Service.Prediction
{
    public class PredictionContext : DbContext
    {
        public DbSet<Models.Prediction> Predictions { get; set; }

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
