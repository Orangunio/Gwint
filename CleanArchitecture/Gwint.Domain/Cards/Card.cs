using Gwint.Domain.Common;

namespace Gwint.Domain.Cards;

/// <summary>
/// Encja Karty (jednocześnie korzeń własnego, małego agregatu - katalogu kart).
/// To wzorcowy przykład "wyleczenia" anemicznego modelu: zamiast publicznych
/// setterów i surowych typów, karta ma niemutowalny stan ustawiany przez
/// fabrykę oraz ZACHOWANIE (np. wyliczenie siły efektywnej, wystawienie
/// reprezentacji do talii).
/// </summary>
public sealed class Card : Entity<CardId>
{
    public CardName Name { get; private set; }
    public Fraction Fraction { get; private set; }
    public Ability Ability { get; private set; }
    public CardStrength Strength { get; private set; }
    public Place Place { get; private set; }
    public bool IsChampion { get; private set; }
    public bool IsCommander { get; private set; }
    public bool IsSpecial { get; private set; }

    // Prywatny konstruktor bezparametrowy dla mechanizmu materializacji ORM.
    // Domena go nie używa - to ustępstwo czysto techniczne (EF tworzy obiekt
    // refleksją), które NIE zanieczyszcza modelu adnotacjami.
    private Card() : base(default!)
    {
        Name = null!;
        Strength = null!;
    }

    private Card(
        CardId id,
        CardName name,
        Fraction fraction,
        Ability ability,
        CardStrength strength,
        Place place,
        bool isChampion,
        bool isCommander,
        bool isSpecial) : base(id)
    {
        Name = name;
        Fraction = fraction;
        Ability = ability;
        Strength = strength;
        Place = place;
        IsChampion = isChampion;
        IsCommander = isCommander;
        IsSpecial = isSpecial;
    }

    /// <summary>
    /// Odtworzenie karty z danych trwałych (używane przez adapter repozytorium).
    /// </summary>
    public static Card Rehydrate(
        int id,
        string name,
        Fraction fraction,
        Ability ability,
        int strength,
        Place place,
        bool isChampion,
        bool isCommander,
        bool isSpecial)
        => new(
            new CardId(id),
            new CardName(name),
            fraction,
            ability,
            new CardStrength(strength),
            place,
            isChampion,
            isCommander,
            isSpecial);

    /// <summary>
    /// Siła efektywna: karty specjalne (pogoda, róg itp.) nie wnoszą siły.
    /// (Zachowanie przeniesione z dawnego CardUseCases.GetEffectiveStrength.)
    /// </summary>
    public int EffectiveStrength() => IsSpecial ? 0 : Strength.Value;
}
