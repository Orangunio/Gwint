using Gwint.Domain.Common;

namespace Gwint.Domain.Players;

/// <summary>
/// Korzeń agregatu Gracz (kontekst uwierzytelniania).
/// Zachowaniowa encja: tworzona przez fabrykę <see cref="Register"/>, która
/// pilnuje niezmienników poprzez Obiekty Wartości (Login, HashedPassword).
/// Nie ma publicznych setterów - stanu nie da się wprowadzić w nieprawidłowy
/// sposób z zewnątrz.
/// </summary>
public sealed class Player : Entity<PlayerId>
{
    public Login Login { get; private set; }
    public HashedPassword Password { get; private set; }

    // Konstruktor techniczny dla materializacji ORM.
    private Player() : base(PlayerId.Unassigned)
    {
        Login = null!;
        Password = null!;
    }

    private Player(PlayerId id, Login login, HashedPassword password) : base(id)
    {
        Login = login;
        Password = password;
    }

    /// <summary>
    /// Rejestracja nowego gracza. Hasło przychodzi już zahaszowane (hashowanie
    /// to szczegół infrastruktury ukryty za portem) - domena gwarantuje jedynie,
    /// że login i hash są poprawnymi obiektami wartości. Identyfikator nada baza.
    /// </summary>
    public static Player Register(Login login, HashedPassword password)
        => new(PlayerId.Unassigned, login, password);

    /// <summary>
    /// Odtworzenie gracza z danych trwałych (używane przez adapter repozytorium).
    /// </summary>
    public static Player Rehydrate(int id, string login, string passwordHash)
        => new(new PlayerId(id), new Login(login), new HashedPassword(passwordHash));
}
