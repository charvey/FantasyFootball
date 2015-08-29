using FantasyFootball.Service.Prediction.Models;
using Microsoft.Data.Entity;

namespace FantasyFootball.Service.Prediction
{
    public class PredictionContext : DbContext
    {
        public DbSet<Model> Models { get; set; }
        public DbSet<Models.Prediction> Predictions { get; set; }
        public DbSet<Run> Runs { get; set; }

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
