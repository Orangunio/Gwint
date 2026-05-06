using Backend.Models;
using Microsoft.EntityFrameworkCore;

namespace Backend.Database
{
    public class GwintDBContext : DbContext
    {
        public DbSet<Models.Player> Players { get; set; }
        public DbSet<Models.Card> Cards { get; set; }
        public DbSet<Models.PlayerDeck> PlayerDecks { get; set; }
        public GwintDBContext(DbContextOptions<GwintDBContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PlayerDeck>()
                .HasOne(pd => pd.Card)
                .WithMany()
                .HasForeignKey(pd => pd.CardId)
                .OnDelete(DeleteBehavior.Restrict);
        }

    }
}
