using Gwint.Application.Abstractions.Persistence;
using Gwint.Domain.Cards;
using Gwint.Domain.Decks;
using Gwint.Domain.Players;
using Gwint.Infrastructure.Persistence.Records;
using Microsoft.EntityFrameworkCore;

namespace Gwint.Infrastructure.Persistence.Repositories;

/// <summary>
/// ADAPTER portu <see cref="IPlayerRepository"/> oparty o EF Core.
/// Tłumaczy między rekordem trwałości a agregatem domeny Player.
/// </summary>
internal sealed class PlayerRepository : IPlayerRepository
{
    private readonly GwintDbContext _db;

    public PlayerRepository(GwintDbContext db) => _db = db;

    public Task<bool> ExistsByLoginAsync(Login login, CancellationToken ct = default)
        => _db.Players.AnyAsync(p => p.Login == login.Value, ct);

    public async Task<Player?> GetByLoginAsync(Login login, CancellationToken ct = default)
    {
        var record = await _db.Players.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Login == login.Value, ct);
        return record is null ? null : ToDomain(record);
    }

    public async Task<Player?> GetByIdAsync(PlayerId id, CancellationToken ct = default)
    {
        var record = await _db.Players.AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id.Value, ct);
        return record is null ? null : ToDomain(record);
    }

    public async Task<Player> AddAsync(Player player, CancellationToken ct = default)
    {
        var record = new PlayerRecord
        {
            Login = player.Login.Value,
            HashPassword = player.Password.Value
        };

        _db.Players.Add(record);
        await _db.SaveChangesAsync(ct);

        // Po zapisie baza nadała Id - odtwarzamy agregat z kompletną tożsamością.
        return ToDomain(record);
    }

    private static Player ToDomain(PlayerRecord r) => Player.Rehydrate(r.Id, r.Login, r.HashPassword);
}

/// <summary>ADAPTER portu <see cref="ICardRepository"/> oparty o EF Core.</summary>
internal sealed class CardRepository : ICardRepository
{
    private readonly GwintDbContext _db;

    public CardRepository(GwintDbContext db) => _db = db;

    public async Task<IReadOnlyList<Card>> GetByFractionAsync(Fraction fraction, CancellationToken ct = default)
    {
        var fractionValue = (int)fraction;
        var records = await _db.Cards.AsNoTracking()
            .Where(c => c.fraction == fractionValue)
            .ToListAsync(ct);
        return records.Select(ToDomain).ToList();
    }

    public async Task<IReadOnlyList<Card>> GetByIdsAsync(IEnumerable<CardId> ids, CancellationToken ct = default)
    {
        var idValues = ids.Select(i => i.Value).Distinct().ToList();
        if (idValues.Count == 0)
            return Array.Empty<Card>();

        var records = await _db.Cards.AsNoTracking()
            .Where(c => idValues.Contains(c.Id))
            .ToListAsync(ct);
        return records.Select(ToDomain).ToList();
    }

    private static Card ToDomain(CardRecord r) => Card.Rehydrate(
        r.Id, r.Name, (Fraction)r.fraction, (Ability)r.ability, r.Strength,
        (Place)r.place, r.isChampion, r.isCommander, r.isSpecial);
}

/// <summary>
/// ADAPTER portu <see cref="IDeckRepository"/>. Składa agregat <see cref="Deck"/>
/// z wielu wierszy tabeli PlayerDecks (złączonych z Cards), a przy zapisie
/// wylicza różnicę i dodaje/usuwa odpowiednie wiersze.
/// </summary>
internal sealed class DeckRepository : IDeckRepository
{
    private readonly GwintDbContext _db;

    public DeckRepository(GwintDbContext db) => _db = db;

    public async Task<Deck> GetAsync(PlayerId ownerId, Fraction fraction, CancellationToken ct = default)
    {
        var fractionValue = (int)fraction;

        var rows = await (
            from pd in _db.PlayerDecks.AsNoTracking()
            join c in _db.Cards.AsNoTracking() on pd.CardId equals c.Id
            where pd.PlayerId == ownerId.Value && c.fraction == fractionValue
            select new { c.Id, c.isCommander, c.isSpecial })
            .ToListAsync(ct);

        var deckCards = rows.Select(r => new DeckCard(new CardId(r.Id), r.isCommander, r.isSpecial));
        return Deck.Rehydrate(ownerId, fraction, deckCards);
    }

    public async Task SaveAsync(Deck deck, CancellationToken ct = default)
    {
        var ownerId = deck.OwnerId.Value;
        var fractionValue = (int)deck.Fraction;

        var existing = await (
            from pd in _db.PlayerDecks
            join c in _db.Cards on pd.CardId equals c.Id
            where pd.PlayerId == ownerId && c.fraction == fractionValue
            select pd)
            .ToListAsync(ct);

        var desiredCardIds = deck.Cards.Select(c => c.CardId.Value).ToHashSet();
        var existingCardIds = existing.Select(pd => pd.CardId).ToHashSet();

        var toRemove = existing.Where(pd => !desiredCardIds.Contains(pd.CardId)).ToList();
        var toAdd = desiredCardIds
            .Where(id => !existingCardIds.Contains(id))
            .Select(id => new PlayerDeckRecord { PlayerId = ownerId, CardId = id })
            .ToList();

        _db.PlayerDecks.RemoveRange(toRemove);
        _db.PlayerDecks.AddRange(toAdd);
        await _db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<CardId>> GetAllCardIdsAsync(PlayerId ownerId, CancellationToken ct = default)
    {
        var ids = await _db.PlayerDecks.AsNoTracking()
            .Where(pd => pd.PlayerId == ownerId.Value)
            .Select(pd => pd.CardId)
            .Distinct()
            .ToListAsync(ct);
        return ids.Select(id => new CardId(id)).ToList();
    }
}
