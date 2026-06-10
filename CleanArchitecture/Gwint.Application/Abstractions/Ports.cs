using Gwint.Domain.Cards;
using Gwint.Domain.Decks;
using Gwint.Domain.Players;

namespace Gwint.Application.Abstractions.Persistence
{
    /// <summary>
    /// PORT repozytorium graczy. Warstwa aplikacji posługuje się tym interfejsem,
    /// nie wiedząc nic o EF Core ani PostgreSQL - konkretną implementację (adapter)
    /// dostarcza infrastruktura. To jest istota odwrócenia zależności.
    /// </summary>
    public interface IPlayerRepository
    {
        Task<bool> ExistsByLoginAsync(Login login, CancellationToken cancellationToken = default);

        Task<Player?> GetByLoginAsync(Login login, CancellationToken cancellationToken = default);

        Task<Player?> GetByIdAsync(PlayerId id, CancellationToken cancellationToken = default);

        /// <summary>Zapisuje nowego gracza i zwraca go z nadanym przez bazę identyfikatorem.</summary>
        Task<Player> AddAsync(Player player, CancellationToken cancellationToken = default);
    }

    /// <summary>PORT katalogu kart (read-only z punktu widzenia tych przypadków użycia).</summary>
    public interface ICardRepository
    {
        Task<IReadOnlyList<Card>> GetByFractionAsync(Fraction fraction, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Card>> GetByIdsAsync(IEnumerable<CardId> ids, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// PORT repozytorium talii. Operuje na całym agregacie <see cref="Deck"/> -
    /// adapter sam tłumaczy agregat na wiersze tabeli PlayerDecks i odwrotnie.
    /// </summary>
    public interface IDeckRepository
    {
        /// <summary>
        /// Zwraca talię gracza dla danej frakcji. Jeśli gracz nie ma jeszcze kart
        /// w tej frakcji, zwraca pustą talię (nigdy null).
        /// </summary>
        Task<Deck> GetAsync(PlayerId ownerId, Fraction fraction, CancellationToken cancellationToken = default);

        /// <summary>Utrwala stan agregatu talii (dodaje/usuwa odpowiednie wiersze PlayerDecks).</summary>
        Task SaveAsync(Deck deck, CancellationToken cancellationToken = default);

        /// <summary>Zwraca tożsamości wszystkich kart przypisanych do gracza (wszystkie frakcje).</summary>
        Task<IReadOnlyList<CardId>> GetAllCardIdsAsync(PlayerId ownerId, CancellationToken cancellationToken = default);
    }
}

namespace Gwint.Application.Abstractions.Security
{
    /// <summary>
    /// PORT hashowania haseł. Domena/aplikacja nie znają BCrypta - posługują się
    /// tą abstrakcją. Adapter (infrastruktura) dostarcza konkretną implementację.
    /// </summary>
    public interface IPasswordHasher
    {
        string Hash(string plainPassword);

        bool Verify(string plainPassword, string passwordHash);
    }

    /// <summary>
    /// PORT generowania tokenów uwierzytelniających. Szczegóły JWT są ukryte
    /// w adapterze infrastruktury.
    /// </summary>
    public interface ITokenService
    {
        string GenerateToken(Player player);
    }
}
