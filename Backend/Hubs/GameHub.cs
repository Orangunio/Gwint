using Backend.Database;
using Backend.Models;
using Backend.Models.Enums;
using Backend.UseCases;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Concurrent;
using System.Numerics;

namespace Backend.Hubs
{
    public class GameHub : Hub
    {
        private static readonly ConcurrentDictionary<string, string> roomToGameConnectionMap = new();
        private readonly GameUseCases gameUseCases;
        private readonly GwintDBContext gwintDBContext;
        private AbilityUseCases abilityUseCases;
        public GameHub(GameUseCases gameUseCases, GwintDBContext gwintDBContext)
        {
            this.gameUseCases = gameUseCases;
            this.gwintDBContext = gwintDBContext;
            this.abilityUseCases = new AbilityUseCases();
        }

        private async Task SendGameStateToAll(string roomId, Game game)
        {
            await Clients.Group(roomId).SendAsync("GameStateUpdated", game);
        }

        public async Task JoinGameRoom(string roomId, string? roomConnectionId = null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            if (roomConnectionId != null)
            {
                roomToGameConnectionMap[roomConnectionId] = Context.ConnectionId;

                if (gameUseCases.games.TryGetValue(roomId, out var game))
                {
                    if (game.Player1?.ConnectionId == roomConnectionId)
                    {
                        game.Player1.ConnectionId = Context.ConnectionId;
                        Console.WriteLine($"[GameHub] Player1 connectionId zaktualizowany: {Context.ConnectionId}");
                    }
                    else if (game.Player2?.ConnectionId == roomConnectionId)
                    {
                        game.Player2.ConnectionId = Context.ConnectionId;
                        Console.WriteLine($"[GameHub] Player2 connectionId zaktualizowany: {Context.ConnectionId}");
                    }
                }
            }
        }

        public async Task StartGame(string roomId, Fractions player1SelectedFraction, Fractions player2SelectedFraction)
        {
            if (!RoomHub.rooms.TryGetValue(roomId, out var room) || room?.Players?.Count != 2)
            {
                await Clients.Caller.SendAsync("Error", "Nieprawidłowy pokój lub brak drugiego gracza.");
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);

            var player1Db = await gwintDBContext.Players
                .FirstOrDefaultAsync(p => p.Login == room.Players[0].Login);
            var player2Db = await gwintDBContext.Players
                .FirstOrDefaultAsync(p => p.Login == room.Players[1].Login);

            if (player1Db == null || player2Db == null)
            {
                await Clients.Caller.SendAsync("Error", "Nie znaleziono graczy w bazie.");
                return;
            }

            var player1Deck = await gwintDBContext.PlayerDecks
                .Where(pd => pd.PlayerId == player1Db.Id)
                .Select(pd => pd.Card)
                .Where(c => c.fraction == player1SelectedFraction)
                .ToListAsync();

            var player2Deck = await gwintDBContext.PlayerDecks
                .Where(pd => pd.PlayerId == player2Db.Id)
                .Select(pd => pd.Card)
                .Where(c => c.fraction == player2SelectedFraction)
                .ToListAsync();

            if (player1Deck.Count == 0 || player2Deck.Count == 0)
            {
                Console.WriteLine("[GameHub] Błąd: Decki są puste.");
            }

            var game = new Game(player1Deck, player2Deck, player1SelectedFraction, player2SelectedFraction);

            game.Player1 = room.Players[0];
            game.Player2 = room.Players[1];
            game.RoomId = roomId;

            game.Player1CommanderCard = player1Deck.FirstOrDefault(c => c.isCommander);
            game.Player2CommanderCard = player2Deck.FirstOrDefault(c => c.isCommander);
            if (game.Player1CommanderCard != null) player1Deck.Remove(game.Player1CommanderCard);
            if (game.Player2CommanderCard != null) player2Deck.Remove(game.Player2CommanderCard);

            game.Player1CardsInDeck = GameUseCases.Shuffle(player1Deck);
            game.Player2CardsInDeck = GameUseCases.Shuffle(player2Deck);

            game.Player1CardsOnHand = game.Player1CardsInDeck.ToList();
            game.Player2CardsOnHand = game.Player2CardsInDeck.ToList();

            if (game.Board == null) game.Board = new Board();

            gameUseCases.games.TryAdd(roomId, game);

            Console.WriteLine($"[GameHub] Gra utworzona | P1: {game.Player1.ConnectionId} | P2: {game.Player2.ConnectionId}");

            await Clients.Group(roomId).SendAsync("GameStarted", game);
            await Clients.Group(roomId).SendAsync("TurnStarted", game.Player1.ConnectionId);
            await SendGameStateToAll(roomId, game);
        }

        public async Task SetFirstPlayer(string roomId, string chosenConnectionId)
        {
            var player = gameUseCases.SetFirstPlayer(roomId, chosenConnectionId);

            await Clients.Group(roomId)
                .SendAsync("TurnStarted", player.ConnectionId);
        }

        public async Task PlayerPass(string roomId)
        {
            var game = gameUseCases.games[roomId];

            if (game.CurrentPlayer == game.Player1)
            {
                game.Player1Passed = true;
                game.CurrentPlayer = game.Player2;
            }
            else
            {
                game.Player2Passed = true;
                game.CurrentPlayer = game.Player1;
            }

            if (game.Player1Passed == true && game.Player2Passed == true)
            {
                await StartNewRound(roomId);
            }
            else
            {
                await Clients.Group(roomId)
                    .SendAsync("NextTurn", game.CurrentPlayer.ConnectionId);
            }

            await SendGameStateToAll(roomId, game);
        }

        public async Task StartNewRound(string roomId)
        {
            var game = gameUseCases.games[roomId];

            if (game.Player1Score > game.Player2Score)
            {
                game.Player1RoundsWon++;
                game.CurrentPlayer = game.Player1;
                if (game.Player1SelectedFraction == Fractions.NorthernRealms)
                {
                    if (game.Player1CardsInDeck.Any())
                    {
                        var card = game.Player1CardsInDeck[0];
                        game.Player1CardsOnHand.Add(card);
                        game.Player1CardsInDeck.RemoveAt(0);
                    }
                }
            }
            else if (game.Player1Score < game.Player2Score)
            {
                game.Player2RoundsWon++;
                game.CurrentPlayer = game.Player2;
                if (game.Player2SelectedFraction == Fractions.NorthernRealms)
                {
                    if (game.Player2CardsInDeck.Any())
                    {
                        var card = game.Player2CardsInDeck[0];
                        game.Player2CardsOnHand.Add(card);
                        game.Player2CardsInDeck.RemoveAt(0);
                    }
                }
            }
            else
            {
                if (game.Player1SelectedFraction == Fractions.Nilfgaard)
                {
                    game.Player1RoundsWon++;
                }
                else if (game.Player2SelectedFraction == Fractions.Nilfgaard)
                {
                    game.Player2RoundsWon++;
                }
                else
                {
                    game.Player1RoundsWon++;
                    game.Player2RoundsWon++;
                }
            }

            if (game.Player1RoundsWon == 2)
            {
                await Clients.Group(roomId)
                    .SendAsync("Player1WonGame", game);
                return;
            }
            else if (game.Player2RoundsWon == 2)
            {
                await Clients.Group(roomId)
                    .SendAsync("Player2WonGame", game);
                return;
            }
            else
            {
                MoveRowOnDisplay(game.Board.Player1FirstCardRow, game.Player1CardsOnDisplay);
                MoveRowOnDisplay(game.Board.Player2FirstCardRow, game.Player2CardsOnDisplay);
                MoveRowOnDisplay(game.Board.Player1SecondCardRow, game.Player1CardsOnDisplay);
                MoveRowOnDisplay(game.Board.Player2SecondCardRow, game.Player2CardsOnDisplay);
                MoveRowOnDisplay(game.Board.Player1ThirdCardRow, game.Player1CardsOnDisplay);
                MoveRowOnDisplay(game.Board.Player2ThirdCardRow, game.Player2CardsOnDisplay);

                game.Player1Passed = false;
                game.Player2Passed = false;

                game.Player1Score = 0;
                game.Player2Score = 0;

                game.Board.FrostActive = false;
                game.Board.FogActive = false;
                game.Board.RainActive = false;

                for (int i = 0; i < 3; i++)
                {
                    game.Board.RogDowodcyActive[0][i] = false;
                    game.Board.RogDowodcyActive[1][i] = false;

                    game.Board.RowScores[0][i] = 0;
                    game.Board.RowScores[1][i] = 0;
                }

                await Clients.Group(roomId).SendAsync("RoundStarted", new
                {
                    game,
                    currentPlayerId = game.CurrentPlayer.ConnectionId
                });
            }
        }

        public void MoveRowOnDisplay(List<Card> row, List<Card> display)
        {
            foreach (Card card in row)
            {
                card.finalStrength = card.Strength;
            }
            display.AddRange(row);
            row.Clear();
        }

        public async Task ChooseFirstPlayer(string roomId)
        {
            var game = gameUseCases.games[roomId];

            bool player1Scoia = game.Player1SelectedFraction == Fractions.ScoiaTael;
            bool player2Scoia = game.Player2SelectedFraction == Fractions.ScoiaTael;

            if (player1Scoia ^ player2Scoia)
            {
                var scoiaPlayer = player1Scoia ? game.Player1 : game.Player2;

                await Clients.Client(scoiaPlayer.ConnectionId)
                    .SendAsync("ScoiataelChooseFirstPlayer");
            }
            else
            {
                var player = gameUseCases.ChooseFirstPlayer(roomId);

                await Clients.Group(roomId)
                    .SendAsync("TurnStarted", player.ConnectionId);
            }
        }

        // Sprawdza czy aktualny gracz ma dowódcę z pasywką losowego wskrzeszenia
        private bool HasRandomResurrectionPassive(Game game)
        {
            return game.Player1CommanderCard?.ability == Abilities.nilfgaard3
                || game.Player2CommanderCard?.ability == Abilities.nilfgaard3;
        }

        public async Task PlayCard(string roomId, Card selectedCardByUser, int? selectedRow = null)
        {
            if (!gameUseCases.games.TryGetValue(roomId, out var game)) return;
            var currentPlayer = game.CurrentPlayer;

            var playerHand = currentPlayer == game.Player1 ? game.Player1CardsOnHand : game.Player2CardsOnHand;
            var cardInHand = playerHand.FirstOrDefault(c => c.Id == selectedCardByUser.Id);

            if (cardInHand == null && !selectedCardByUser.isCommander) return;
            if (cardInHand != null) playerHand.Remove(cardInHand);

            if (!selectedCardByUser.isSpecial && !selectedCardByUser.isCommander)
            {
                if (selectedCardByUser.ability != Abilities.szpieg && selectedCardByUser.ability != Abilities.zwinnośc)
                {
                    abilityUseCases.AddCardToBoard(game, currentPlayer, selectedCardByUser.place, selectedCardByUser);
                }
            }

            await ExecuteCardAbility(roomId, game, selectedCardByUser, selectedRow);

            bool needsInteraction =
                selectedCardByUser.ability == Abilities.manekinDoCwiczen ||
                (selectedCardByUser.ability == Abilities.wskrzeszenie && HasRevivableCards(game) && !HasRandomResurrectionPassive(game)) ||
                (selectedCardByUser.ability == Abilities.zwinnośc && selectedRow == null) ||
                (selectedCardByUser.ability == Abilities.rogDowodcy && selectedRow == null) ||
                (selectedCardByUser.isCommander && selectedCardByUser.ability == Abilities.nilfgaard1);

            if (!needsInteraction)
            {
                await FinishTurn(roomId, game);
            }
        }

        private async Task FinishTurn(string roomId, Game game)
        {
            UpdateCardsFinalStrength(game);

            game.Board.CalculateRowScores();
            if (game.CurrentPlayer == game.Player1)
            {
                game.CurrentPlayer = game.Player2Passed ? game.Player1 : game.Player2;
            }
            else
            {
                game.CurrentPlayer = game.Player1Passed ? game.Player2 : game.Player1;
            }

            await Clients.Group(roomId).SendAsync("NextTurn", game.CurrentPlayer.ConnectionId);
            await SendGameStateToAll(roomId, game);
        }

        private static void UpdateCardsFinalStrength(Game game)
        {
            game.Board.CalculateRow(game.Board.Player1FirstCardRow, game.Board.FrostActive, game.Board.RogDowodcyActive[0][0]);
            game.Board.CalculateRow(game.Board.Player1SecondCardRow, game.Board.FogActive, game.Board.RogDowodcyActive[0][1]);
            game.Board.CalculateRow(game.Board.Player1ThirdCardRow, game.Board.RainActive, game.Board.RogDowodcyActive[0][2]);

            game.Board.CalculateRow(game.Board.Player2FirstCardRow, game.Board.FrostActive, game.Board.RogDowodcyActive[1][0]);
            game.Board.CalculateRow(game.Board.Player2SecondCardRow, game.Board.FogActive, game.Board.RogDowodcyActive[1][1]);
            game.Board.CalculateRow(game.Board.Player2ThirdCardRow, game.Board.RainActive, game.Board.RogDowodcyActive[1][2]);

            game.Player1Score = game.Board.GetPlayerTotalScore(true);
            game.Player2Score = game.Board.GetPlayerTotalScore(false);
        }

        private bool HasRevivableCards(Game game)
        {
            var graveyard = game.CurrentPlayer == game.Player1 ? game.Player1CardsOnDisplay : game.Player2CardsOnDisplay;
            return graveyard.Any(c => !c.isChampion && !c.isSpecial);
        }

        public async Task ResolveResurrection(string roomId, int cardToReviveId)
        {
            if (!gameUseCases.games.TryGetValue(roomId, out var game)) return;
            var graveyard = game.CurrentPlayer == game.Player1 ? game.Player1CardsOnDisplay : game.Player2CardsOnDisplay;
            var cardToRevive = graveyard.FirstOrDefault(c => c.Id == cardToReviveId);

            if (cardToRevive != null)
            {
                graveyard.Remove(cardToRevive);
                if (cardToRevive.ability != Abilities.zwinnośc && cardToRevive.ability != Abilities.szpieg)
                    abilityUseCases.AddCardToBoard(game, game.CurrentPlayer, cardToRevive.place, cardToRevive);

                await ExecuteCardAbility(roomId, game, cardToRevive);
                if (cardToRevive.ability != Abilities.wskrzeszenie && cardToRevive.ability != Abilities.zwinnośc)
                {
                    await FinishTurn(roomId, game);
                }
                else
                {
                    await SendGameStateToAll(roomId, game);
                }
            }
        }

        public async Task ResolveDecoy(string roomId, int targetCardId)
        {
            if (!gameUseCases.games.TryGetValue(roomId, out var game)) return;

            var playerRows = game.CurrentPlayer == game.Player1
                ? new[] { game.Board.Player1FirstCardRow, game.Board.Player1SecondCardRow, game.Board.Player1ThirdCardRow }
                : new[] { game.Board.Player2FirstCardRow, game.Board.Player2SecondCardRow, game.Board.Player2ThirdCardRow };

            Card? cardOnBoard = null;
            foreach (var row in playerRows)
            {
                cardOnBoard = row.FirstOrDefault(c => c.Id == targetCardId);
                if (cardOnBoard != null && !cardOnBoard.isChampion)
                {
                    cardOnBoard.finalStrength = cardOnBoard.Strength;
                    abilityUseCases.ManekinDoCwiczenAbility(game, cardOnBoard);
                    await FinishTurn(roomId, game);
                    await SendGameStateToAll(roomId, game);
                    break;
                }
            }
        }

        public async Task ResolveHorn(string roomId, int row)
        {
            var game = gameUseCases.games[roomId];

            Console.WriteLine($"[HORN] Row: {row}");

            abilityUseCases.RogDowodcy(game, row);

            await FinishTurn(roomId, game);
            await SendGameStateToAll(roomId, game);
        }

        public async Task ConfirmReveal(string roomId)
        {
            if (!gameUseCases.games.TryGetValue(roomId, out var game)) return;
            await FinishTurn(roomId, game);
        }

        private async Task ExecuteCardAbility(string roomId, Game game, Card card, int? selectedRow = null)
        {
            if (card.isCommander)
            {
                switch (card.ability)
                {
                    case Abilities.nilfgaard1:
                        var opponentHand = game.CurrentPlayer == game.Player1
                            ? game.Player2CardsOnHand
                            : game.Player1CardsOnHand;
                        var revealedCards = abilityUseCases.EmperorOfNilfgardAbility(opponentHand);
                        await Clients.Caller.SendAsync("RevealOpponentCards", revealedCards);
                        break;
                    case Abilities.nilfgaard2: abilityUseCases.HisEmperialmajestyAbility(game); break;
                    case Abilities.nilfgaard3:
                        // Pasywka — nie robi nic przy zagraniu, działa przy Wskrzeszeniu
                        break;
                    case Abilities.nilfgaard4: abilityUseCases.TheRelentlessAbility(game, card); break;
                    case Abilities.nilfgaard5: abilityUseCases.TheWhiteFlameAbility(game); break;
                    case Abilities.polnoc1: abilityUseCases.KingOfTemeriaAbility(game); break;
                    case Abilities.polnoc2: abilityUseCases.CommanderOfTheNorthAbility(game); break;
                    case Abilities.polnoc3: abilityUseCases.SonOfMedellAbility(game); break;
                    case Abilities.polnoc4: abilityUseCases.TheSiegemasterAbility(game); break;
                    case Abilities.polnoc5: abilityUseCases.TheSteelForged(game); break;
                }
            }
            else
            {
                switch (card.ability)
                {
                    case Abilities.trzaskajacyMroz: abilityUseCases.TrzaskajacyMrozAbility(game); break;
                    case Abilities.gestaMgla: abilityUseCases.GestaMglaAbility(game); break;
                    case Abilities.ulewnyDeszcz: abilityUseCases.UlewnyDeszczAbility(game); break;
                    case Abilities.czysteNiebo: abilityUseCases.CzysteNieboAbility(game); break;
                    case Abilities.pozoga: abilityUseCases.Pozoga(game); break;
                    case Abilities.rogDowodcy:
                        if (selectedRow == null)
                            await Clients.Caller.SendAsync("RequestHornRow", card.Id);
                        else
                            abilityUseCases.RogDowodcy(game, (int)selectedRow);
                        break;
                    case Abilities.manekinDoCwiczen:
                        await Clients.Caller.SendAsync("SelectCardToDecoy", card.Id);
                        break;
                    case Abilities.wskrzeszenie:
                        if (HasRevivableCards(game))
                        {
                            // Jeśli gracz ma pasywkę Najezdnika Północy — wskrzeszamy losowo
                            if (HasRandomResurrectionPassive(game))
                            {
                                var graveyard = game.CurrentPlayer == game.Player1
                                    ? game.Player1CardsOnDisplay
                                    : game.Player2CardsOnDisplay;

                                var revivable = graveyard
                                    .Where(c => !c.isChampion && !c.isSpecial)
                                    .ToList();

                                var rnd = new Random();
                                var randomCard = revivable[rnd.Next(revivable.Count)];

                                graveyard.Remove(randomCard);
                                if (randomCard.ability != Abilities.zwinnośc && randomCard.ability != Abilities.szpieg)
                                    abilityUseCases.AddCardToBoard(game, game.CurrentPlayer, randomCard.place, randomCard);

                                await ExecuteCardAbility(roomId, game, randomCard);

                                // Informujemy gracza jaką kartę wskrzeszono
                                await Clients.Caller.SendAsync("RandomResurrectionResult", randomCard);
                            }
                            else
                            {
                                await Clients.Caller.SendAsync("RequestResurrectionTarget");
                            }
                        }
                        break;
                    case Abilities.zwinnośc:
                        if (selectedRow == null)
                            await Clients.Caller.SendAsync("RequestAgilityRow", card.Id);
                        else
                            abilityUseCases.ZwinnoscAbility(game, card, (int)selectedRow);
                        break;
                    case Abilities.szpieg:
                        abilityUseCases.SzpiegAbility(game, card);
                        break;
                    case Abilities.braterstwo:
                        abilityUseCases.BraterstwoAbility(game, card);
                        break;
                    case Abilities.pozogaJednostki:
                        abilityUseCases.PozogaJednostkiAbility(game, card);
                        break;
                    default:
                        abilityUseCases.WiezAbility(game);
                        abilityUseCases.WyzszeMoraleAbility(game);
                        break;
                }
            }
        }
    }
}