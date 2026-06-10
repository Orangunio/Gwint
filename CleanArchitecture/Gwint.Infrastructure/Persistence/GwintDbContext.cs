using Gwint.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace Gwint.Infrastructure.Persistence;

/// <summary>
/// Kontekst EF Core. Widzi WYŁĄCZNIE modele trwałości (POCO), nigdy encji
/// domeny - cała wiedza o ORM jest zamknięta w warstwie infrastruktury.
/// Mapowanie na istniejące tabele bazy realizujemy przez Fluent API.
/// </summary>
internal sealed class GwintDbContext : DbContext
{
    public DbSet<PlayerRecord> Players => Set<PlayerRecord>();
    public DbSet<CardRecord> Cards => Set<CardRecord>();
    public DbSet<PlayerDeckRecord> PlayerDecks => Set<PlayerDeckRecord>();

    public GwintDbContext(DbContextOptions<GwintDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("public");

        modelBuilder.Entity<PlayerRecord>(entity =>
        {
            entity.ToTable("Players", "public");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            entity.Property(p => p.Login).HasColumnName("Login").IsRequired();
            entity.Property(p => p.HashPassword).HasColumnName("HashPassword").IsRequired();
        });

        modelBuilder.Entity<CardRecord>(entity =>
        {
            entity.ToTable("Cards", "public");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            entity.Property(c => c.Name).HasColumnName("Name").IsRequired();
            entity.Property(c => c.fraction).HasColumnName("fraction");
            entity.Property(c => c.ability).HasColumnName("ability");
            entity.Property(c => c.Strength).HasColumnName("Strength");
            entity.Property(c => c.place).HasColumnName("place");
            entity.Property(c => c.isChampion).HasColumnName("isChampion");
            entity.Property(c => c.isCommander).HasColumnName("isCommander");
            entity.Property(c => c.isSpecial).HasColumnName("isSpecial");
        });

        modelBuilder.Entity<PlayerDeckRecord>(entity =>
        {
            entity.ToTable("PlayerDecks", "public");
            entity.HasKey(pd => pd.Id);
            entity.Property(pd => pd.Id).HasColumnName("Id").ValueGeneratedOnAdd();
            entity.Property(pd => pd.PlayerId).HasColumnName("PlayerId");
            entity.Property(pd => pd.CardId).HasColumnName("CardId");
            entity.HasIndex(pd => pd.PlayerId);
            entity.HasIndex(pd => pd.CardId);
        });
    }
}
