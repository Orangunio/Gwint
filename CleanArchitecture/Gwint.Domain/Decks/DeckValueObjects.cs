using Gwint.Domain.Cards;
using Gwint.Domain.Common;
using Gwint.Domain.Players;

namespace Gwint.Domain.Decks;

/// <summary>
/// Tożsamość talii. Talia jest jednoznacznie wyznaczona przez właściciela
/// (gracza) oraz frakcję - każdy gracz ma jedną talię na frakcję.
/// </summary>
public sealed class DeckId : ValueObject
{
    public PlayerId OwnerId { get; }
    public Fraction Fraction { get; }

    public DeckId(PlayerId ownerId, Fraction fraction)
    {
        OwnerId = ownerId;
        Fraction = fraction;
    }

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return OwnerId;
        yield return Fraction;
    }

    public override string ToString() => $"{OwnerId}:{Fraction}";
}

/// <summary>
/// Pozycja w talii - Value Object wewnątrz agregatu talii.
///
/// KLUCZOWE dla reguły "agregaty komunikują się przez tożsamość, nie przez
/// referencje obiektowe": talia NIE trzyma obiektu <see cref="Card"/>. Trzyma
/// jedynie <see cref="Cards.CardId"/> (tożsamość karty z innego agregatu) oraz
/// minimalny zestaw cech potrzebnych do pilnowania własnych niezmienników składu
/// talii (czy to dowódca, czy karta specjalna).
/// </summary>
public sealed class DeckCard : ValueObject
{
    public CardId CardId { get; }
    public bool IsCommander { get; }
    public bool IsSpecial { get; }

    public DeckCard(CardId cardId, bool isCommander, bool isSpecial)
    {
        CardId = cardId;
        IsCommander = isCommander;
        IsSpecial = isSpecial;
    }

    // W obrębie talii pozycję identyfikuje tożsamość karty - dzięki temu ta sama
    // karta nie może trafić do talii dwa razy.
    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return CardId;
    }
}
