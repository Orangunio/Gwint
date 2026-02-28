using Microsoft.EntityFrameworkCore;

namespace Backend.Database
{
    public class GwintDBContext : DbContext
    {
        public DbSet<Models.Player> Players { get; set; }
        public GwintDBContext(DbContextOptions<GwintDBContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("public");
            base.OnModelCreating(modelBuilder);
        }

    }
}
