# Gwint — DDD + Clean Architecture (Pracownia 10/11)

Refaktoryzacja dwóch wycinków funkcjonalności gry Gwint do modelu DDD i
architektury portów i adapterów (Clean Architecture):

1. **Budowanie talii** (PlayerDeck + Card)
2. **Rejestracja / logowanie gracza** (auth)

## Struktura (kierunek zależności wymuszony przez referencje projektów)

```
Gwint.Domain  ←  Gwint.Application  ←  Gwint.Infrastructure
                        ↑                       ↑
                        └────── Gwint.Api ──────┘   (Composition Root)
```

- **Gwint.Domain** — model domeny, ZERO zależności od technologii (brak EF,
  ASP.NET, BCrypt). Encje, Obiekty Wartości, agregaty, reguły biznesowe.
- **Gwint.Application** — porty (interfejsy) + przypadki użycia. Zna tylko domenę.
- **Gwint.Infrastructure** — adaptery: EF Core (PostgreSQL), BCrypt, JWT.
  Implementuje porty z warstwy aplikacji.
- **Gwint.Api** — cienkie kontrolery + DTO zachowujące kontrakt frontendu;
  punkt kompozycji wiążący porty z adapterami (DI).

## Jak zadanie jest zrealizowane

### 1. Leczenie anemicznego modelu domeny
- **Obiekty Wartości** (niemutowalne, równość przez wartość):
  `Login`, `HashedPassword`, `PlayerId`, `CardId`, `CardName`, `CardStrength`,
  `DeckCard`, `DeckId` (klasa bazowa w `Gwint.Domain/Common/DomainBase.cs`).
- **Zachowaniowe encje** zamiast struktur z setterami: `Player` (tworzony przez
  fabrykę `Register`, bez publicznych setterów), `Card` (zachowanie
  `EffectiveStrength`).
- **Reguły walidacji talii** (dokładnie 1 dowódca, max 10 kart specjalnych,
  min. liczba kart) przeniesione z `PlayerDeckController` do agregatu `Deck`
  (`Deck.EnsureValidComposition`).

### 2. Agregaty i komunikacja przez tożsamość (ID)
- Korzeń agregatu `Deck` przechowuje wyłącznie `DeckCard` = `CardId` + cechy
  potrzebne do niezmienników. **Nie trzyma referencji do obiektów `Card`** —
  agregaty komunikują się przez ID (`Gwint.Domain/Decks/`).
- `Deck` odwołuje się do właściciela przez `PlayerId`, nie przez obiekt `Player`.

### 3. Porty i adaptery + odwrócenie zależności
- **Porty** w `Gwint.Application/Abstractions/`: `IPlayerRepository`,
  `ICardRepository`, `IDeckRepository`, `IPasswordHasher`, `ITokenService`.
- **Adaptery** w `Gwint.Infrastructure/`: repozytoria EF Core, `BCryptPasswordHasher`,
  `JwtTokenService`. Domena nie wie o EF — EF widzi tylko osobne modele
  trwałości (`Persistence/Records/`), mapowane Fluent API na istniejące tabele.
- Wiązanie portów z adapterami: `Gwint.Infrastructure/DependencyInjection.cs`
  + `Gwint.Api/Program.cs`.

```bash
dotnet build Gwint.CleanArch.sln
dotnet run --project Gwint.Api        # http://localhost:5006
```
