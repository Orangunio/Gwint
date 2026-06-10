using Gwint.Domain.Cards;
using Gwint.Domain.Common;

namespace Gwint.Application.Decks;

/// <summary>
/// DTO karty dla puli kart (endpoint available-cards). Frakcja/umiejętność/rząd
/// jako int - zgodnie z kontraktem frontendu.
/// </summary>
public sealed record CardDto(
    int Id,
    string Name,
    int Fraction,
    int Ability,
    int Strength,
    int Place,
    bool IsChampion,
    bool IsCommander,
    bool IsSpecial)
{
    public static CardDto FromDomain(Card card) => new(
        card.Id.Value,
        card.Name.Value,
        (int)card.Fraction,
        (int)card.Ability,
        card.Strength.Value,
        (int)card.Place,
        card.IsChampion,
        card.IsCommander,
        card.IsSpecial);
}

/// <summary>
/// DTO pozycji talii (endpointy listujące talię gracza). Zawiera CardId - to
/// pole, którego używa frontend do identyfikacji kart w talii.
/// </summary>
public sealed record DeckCardDto(
    int CardId,
    string Name,
    int Fraction,
    int Ability,
    int Strength,
    int Place,
    bool IsChampion,
    bool IsCommander,
    bool IsSpecial)
{
    public static DeckCardDto FromDomain(Card card) => new(
        card.Id.Value,
        card.Name.Value,
        (int)card.Fraction,
        (int)card.Ability,
        card.Strength.Value,
        (int)card.Place,
        card.IsChampion,
        card.IsCommander,
        card.IsSpecial);
}

/// <summary>
/// Komenda aktualizacji talii gracza dla danej frakcji - odwzorowuje payload
/// z frontendu (playerId, fraction, cardIdsToAdd, cardIdsToRemove).
/// </summary>
public sealed record UpdateDeckCommand(
    int PlayerId,
    int Fraction,
    IReadOnlyList<int> CardIdsToAdd,
    IReadOnlyList<int> CardIdsToRemove);

/// <summary>
/// Tłumaczy liczbę frakcji (z kontraktu frontendu) na enum domeny,
/// pilnując poprawności wejścia.
/// </summary>
public static class FractionParser
{
    public static Fraction FromInt(int value)
    {
        if (!Enum.IsDefined(typeof(Fraction), value))
            throw new DomainException("Nieprawidłowa frakcja.");

        return (Fraction)value;
    }
}
