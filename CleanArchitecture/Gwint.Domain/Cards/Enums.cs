namespace Gwint.Domain.Cards;

/// <summary>
/// Enumy kart. Wartości liczbowe są zgodne ze schematem bazy danych
/// (kolumny fraction/ability/place) oraz kontraktem frontendu.
/// </summary>
public enum Fraction
{
    Nilfgaard = 1,
    NorthernRealms = 2,
    ScoiaTael = 3,
    Monsters = 4,
}

public enum Ability
{
    None = 0,

    // Umiejętności kart jednostek
    Braterstwo = 1,
    Szpieg = 2,
    Zwinnosc = 3,
    Wskrzeszenie = 4,
    Wiez = 5,
    WyzszeMorale = 6,
    PozogaJednostki = 7,
    RogDowodcyJednostki = 8,
    BydleceSilyZbrojne = 9,

    // Umiejętności kart specjalnych
    RogDowodcy = 10,
    Pozoga = 11,
    ManekinDoCwiczen = 12,
    TrzaskajacyMroz = 13,
    GestaMgla = 14,
    UlewnyDeszcz = 15,
    CzysteNiebo = 16,

    // Umiejętności dowódców
    Nilfgaard1 = 17,
    Nilfgaard2 = 18,
    Nilfgaard3 = 19,
    Nilfgaard4 = 20,
    Nilfgaard5 = 21,

    Polnoc1 = 22,
    Polnoc2 = 23,
    Polnoc3 = 24,
    Polnoc4 = 25,
    Polnoc5 = 26,

    Scolliatel1 = 27,
    Scolliatel2 = 28,
    Scolliatel3 = 29,
    Scolliatel4 = 30,
    Scolliatel5 = 31,

    Monsters1 = 32,
    Monsters2 = 33,
    Monsters3 = 34,
    Monsters4 = 35,
    Monsters5 = 36,
}

public enum Place
{
    WithoutRow = 0,
    FirstRow = 1,
    SecondRow = 2,
    ThirdRow = 3,
    FirstAndSecondRow = 12,
    SecondAndThirdRow = 23,
    FirstAndThirdRow = 13,
    AllRows = 123,
}
