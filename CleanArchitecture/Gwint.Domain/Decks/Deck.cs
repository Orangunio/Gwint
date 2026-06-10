using Gwint.Domain.Cards;
using Gwint.Domain.Common;
using Gwint.Domain.Players;

namespace Gwint.Domain.Decks;

/// <summary>
/// Korzeń agregatu Talia (kontekst budowania talii).
///
/// To główny przykład "wyleczenia" anemii: reguły poprawności talii, które
/// wcześniej były rozsiane po metodzie kontrolera (PlayerDeckController.UpdateDeck),
/// żyją teraz w domenie jako zachowanie agregatu. Świat zewnętrzny zmienia
/// skład talii WYŁĄCZNIE przez metody korzenia (AddCard/RemoveCard), które
/// pilnują niezmienników. Talia odwołuje się do kart innego agregatu tylko
/// przez ich tożsamość (CardId).
/// </summary>
public sealed class Deck : Entity<DeckId>
{
    // Reguły składu talii (dawniej zaszyte w kontrolerze).
    public const int RequiredCommanders = 1;
    public const int MaxSpecialCards = 10;
    public const int MinCards = 26;

    private readonly List<DeckCard> _cards = new();

    public IReadOnlyCollection<DeckCard> Cards => _cards.AsReadOnly();

    public PlayerId OwnerId => Id.OwnerId;
    public Fraction Fraction => Id.Fraction;

    private Deck(DeckId id, IEnumerable<DeckCard> cards) : base(id)
    {
        _cards.AddRange(cards);
    }

    /// <summary>Pusta talia gracza dla wskazanej frakcji.</summary>
    public static Deck Empty(PlayerId ownerId, Fraction fraction)
        => new(new DeckId(ownerId, fraction), Enumerable.Empty<DeckCard>());

    /// <summary>Odtworzenie talii z danych trwałych (adapter repozytorium).</summary>
    public static Deck Rehydrate(PlayerId ownerId, Fraction fraction, IEnumerable<DeckCard> cards)
        => new(new DeckId(ownerId, fraction), cards);

    public int CommanderCount => _cards.Count(c => c.IsCommander);
    public int SpecialCount => _cards.Count(c => c.IsSpecial);
    public int CardCount => _cards.Count;

    public bool Contains(CardId cardId) => _cards.Any(c => c.CardId == cardId);

    /// <summary>
    /// Dodaje kartę do talii. Karta musi należeć do frakcji talii, a tej samej
    /// karty nie można dodać dwukrotnie. Operacja jest idempotentna względem
    /// ponownego dodania tej samej karty.
    /// </summary>
    public void AddCard(Card card)
    {
        if (card.Fraction != Fraction)
            throw new DomainException(
                $"Karta '{card.Name}' należy do frakcji {card.Fraction}, a talia jest frakcji {Fraction}.");

        if (Contains(card.Id))
            return;

        // Talia zatrzymuje wyłącznie tożsamość karty (CardId) i cechy istotne
        // dla własnych reguł - nie przechowuje referencji do obiektu Card.
        _cards.Add(new DeckCard(card.Id, card.IsCommander, card.IsSpecial));
    }

    /// <summary>Usuwa kartę o wskazanej tożsamości (jeśli jest w talii).</summary>
    public void RemoveCard(CardId cardId)
    {
        _cards.RemoveAll(c => c.CardId == cardId);
    }

    /// <summary>
    /// Pilnuje niezmienników poprawnej talii. Rzuca <see cref="DomainException"/>,
    /// jeśli skład jest nieprawidłowy. To jeden punkt prawdy dla reguł talii.
    /// </summary>
    public void EnsureValidComposition()
    {
        if (CommanderCount != RequiredCommanders)
            throw new DomainException("Talia musi zawierać dokładnie jednego dowódcę.");

        if (SpecialCount > MaxSpecialCards)
            throw new DomainException($"Talia może zawierać maksymalnie {MaxSpecialCards} kart specjalnych.");

        if (CardCount < MinCards)
            throw new DomainException($"Talia musi zawierać minimum {MinCards} kart (łącznie z dowódcą).");
    }

    public bool IsValid()
    {
        try
        {
            EnsureValidComposition();
            return true;
        }
        catch (DomainException)
        {
            return false;
        }
    }
}
