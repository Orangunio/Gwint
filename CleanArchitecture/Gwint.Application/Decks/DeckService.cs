using Gwint.Application.Abstractions.Persistence;
using Gwint.Domain.Cards;
using Gwint.Domain.Players;

namespace Gwint.Application.Decks;

/// <summary>
/// Przypadki użycia budowania talii. Logika walidacji składu talii NIE żyje
/// już tutaj - została przeniesiona do agregatu <see cref="Domain.Decks.Deck"/>.
/// Serwis jedynie orkiestruje: ładuje agregat, zleca mu zmiany, prosi o
/// sprawdzenie niezmienników i utrwala wynik.
/// </summary>
public sealed class DeckService
{
    private readonly IDeckRepository _decks;
    private readonly ICardRepository _cards;

    public DeckService(IDeckRepository decks, ICardRepository cards)
    {
        _decks = decks;
        _cards = cards;
    }

    /// <summary>Przypadek użycia: pula wszystkich kart danej frakcji.</summary>
    public async Task<IReadOnlyList<CardDto>> GetAvailableCardsAsync(int fraction, CancellationToken ct = default)
    {
        var fractionEnum = FractionParser.FromInt(fraction);
        var cards = await _cards.GetByFractionAsync(fractionEnum, ct);
        return cards.Select(CardDto.FromDomain).ToList();
    }

    /// <summary>Przypadek użycia: talia gracza w danej frakcji.</summary>
    public async Task<IReadOnlyList<DeckCardDto>> GetFractionDeckAsync(int playerId, int fraction, CancellationToken ct = default)
    {
        var fractionEnum = FractionParser.FromInt(fraction);
        var deck = await _decks.GetAsync(new PlayerId(playerId), fractionEnum, ct);
        return await MapDeckCardsAsync(deck.Cards.Select(c => c.CardId), ct);
    }

    /// <summary>Przypadek użycia: wszystkie karty gracza (wszystkie frakcje).</summary>
    public async Task<IReadOnlyList<DeckCardDto>> GetPlayerDeckAsync(int playerId, CancellationToken ct = default)
    {
        var cardIds = await _decks.GetAllCardIdsAsync(new PlayerId(playerId), ct);
        return await MapDeckCardsAsync(cardIds, ct);
    }

    /// <summary>
    /// Przypadek użycia: aktualizacja talii. Wczytuje agregat, nanosi zmiany
    /// przez jego metody, weryfikuje niezmienniki i zapisuje.
    /// </summary>
    public async Task<IReadOnlyList<DeckCardDto>> UpdateDeckAsync(UpdateDeckCommand command, CancellationToken ct = default)
    {
        var fractionEnum = FractionParser.FromInt(command.Fraction);
        var ownerId = new PlayerId(command.PlayerId);

        var deck = await _decks.GetAsync(ownerId, fractionEnum, ct);

        foreach (var cardId in command.CardIdsToRemove ?? Array.Empty<int>())
            deck.RemoveCard(new CardId(cardId));

        var idsToAdd = (command.CardIdsToAdd ?? Array.Empty<int>())
            .Distinct()
            .Select(id => new CardId(id))
            .ToList();

        if (idsToAdd.Count > 0)
        {
            var cardsToAdd = await _cards.GetByIdsAsync(idsToAdd, ct);
            // Dodajemy tylko karty należące do frakcji talii (tak jak dawniej).
            foreach (var card in cardsToAdd.Where(c => c.Fraction == fractionEnum))
                deck.AddCard(card);
        }

        // Niezmienniki talii pilnuje sam agregat.
        deck.EnsureValidComposition();

        await _decks.SaveAsync(deck, ct);

        return await MapDeckCardsAsync(deck.Cards.Select(c => c.CardId), ct);
    }

    private async Task<IReadOnlyList<DeckCardDto>> MapDeckCardsAsync(IEnumerable<CardId> cardIds, CancellationToken ct)
    {
        var ids = cardIds.ToList();
        if (ids.Count == 0)
            return Array.Empty<DeckCardDto>();

        var cards = await _cards.GetByIdsAsync(ids, ct);
        return cards.Select(DeckCardDto.FromDomain).ToList();
    }
}
