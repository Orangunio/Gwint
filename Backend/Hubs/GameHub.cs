using Backend.Database;
using Backend.Models;
using Backend.Models.Enums;
using Backend.UseCases;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Numerics;

namespace Backend.Hubs
{
    public class GameHub : Hub
    {
        private readonly GameUseCases gameUseCases;
        private readonly GwintDBContext gwintDBContext;
        private AbilityUseCases abilityUseCases;
        public GameHub(GameUseCases gameUseCases, GwintDBContext gwintDBContext) 
        { 
            this.gameUseCases = gameUseCases;
            this.gwintDBContext = gwintDBContext;
            this.abilityUseCases = new AbilityUseCases();
        }

        public async Task JoinGameRoom(string roomId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, roomId);
        }

        public async Task StartGame(string roomId, Fractions player1SelectedFraction, Fractions player2SelectedFraction)
        {
            if (RoomHub.rooms.TryGetValue(roomId, out var room) && room.Players.Count == 2)
            {
                var player1Id = room.Players[0].Id;
                var player2Id = room.Players[1].Id;

                // Pobieramy talie z OrderBy, żeby uniknąć ostrzeżeń EF Core
                var player1Deck = await gwintDBContext.PlayerDecks
                    .Where(pd => pd.PlayerId == player1Id)
                    .Include(pd => pd.Card)
                    .Where(pd => pd.Card.fraction == player1SelectedFraction)
                    .OrderBy(pd => pd.CardId) 
                    .Select(pd => pd.Card)
                    .Take(25) // Zwiększ do 25, żeby mieć zapas na dowódcę + 10 do ręki + resztę w talii
                    .ToListAsync();

                var player2Deck = await gwintDBContext.PlayerDecks
                    .Where(pd => pd.PlayerId == player2Id)
                    .Include(pd => pd.Card)
                    .Where(pd => pd.Card.fraction == player2SelectedFraction)
                    .OrderBy(pd => pd.CardId)
                    .Select(pd => pd.Card)
                    .Take(25)
                    .ToListAsync();

                var game = new Game(player1Deck, player2Deck, player1SelectedFraction, player2SelectedFraction);
                game.Player1 = room.Players[0];
                game.Player2 = room.Players[1];
                game.RoomId = roomId;

                // BEZPIECZNE PRZYPISANIE DOWÓDCÓW
                game.Player1CommanderCard = player1Deck.FirstOrDefault(c => c.isCommander);
                game.Player2CommanderCard = player2Deck.FirstOrDefault(c => c.isCommander);

                // Usuwamy tylko jeśli znaleziono dowódcę
                if (game.Player1CommanderCard != null) player1Deck.Remove(game.Player1CommanderCard);
                if (game.Player2CommanderCard != null) player2Deck.Remove(game.Player2CommanderCard);

                // Przypisuje karty do talii (Upewnij się, że Shuffle zwraca List<Card>)
                game.Player1CardInDeck = GameUseCases.Shuffle(player1Deck);
                game.Player2CardInDeck = GameUseCases.Shuffle(player2Deck);

                // Rozdanie kart (Take(10) jest bezpieczne nawet jeśli jest mniej kart)
                game.Player1CardsOnHand = game.Player1CardInDeck.Take(10).ToList();
                game.Player2CardsOnHand = game.Player2CardInDeck.Take(10).ToList();
                
                game.Player1CardInDeck = game.Player1CardInDeck.Skip(10).ToList();
                game.Player2CardInDeck = game.Player2CardInDeck.Skip(10).ToList();

                // INICJALIZACJA PLANSZY (Jeśli Board w modelu Game jest nullem, frontend się nie wyświetli)
                if (game.Board == null) game.Board = new Board();

                gameUseCases.games.TryAdd(roomId, game);

                // Wysyłamy informację o starcie
                await Clients.Group(roomId).SendAsync("GameStarted", game);
                
                // Opcjonalnie: Od razu powiedz czyja tura (zazwyczaj gracza 1)
                await Clients.Group(roomId).SendAsync("TurnStarted", game.Player1.ConnectionId);
            }
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

            var player = game.CurrentPlayer;

            if(game.CurrentPlayer == game.Player1)
            {
                game.Player1Passed = true;
                game.CurrentPlayer = game.Player2;
            }
            else
            {
                game.Player2Passed = true;
                game.CurrentPlayer = game.Player1;
            }

            if(game.Player1Passed == true &&  game.Player2Passed == true)
            {
                //Powinien byc tutaj reset rundy
            }
            else
            {
                await Clients.Group(roomId)
                    .SendAsync("NextTurn", game.CurrentPlayer.ConnectionId);
            }
        }

        public async Task ChooseFirstPlayer(string roomId)
        {
            var game = gameUseCases.games[roomId];

            bool player1Scoia = game.Player1SelectedFraction == Fractions.ScoiaTael;
            bool player2Scoia = game.Player2SelectedFraction == Fractions.ScoiaTael;

            // jeśli dokładnie jeden ma Scoia'tael
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
            bool needsInteraction = selectedCardByUser.ability == Abilities.manekinDoCwiczen ||
                                   (selectedCardByUser.ability == Abilities.wskrzeszenie && HasRevivableCards(game)) ||
                                   (selectedCardByUser.ability == Abilities.zwinnośc && selectedRow == null);

            if (!needsInteraction)
            {
                await FinishTurn(roomId, game);
            }
        }
        private async Task FinishTurn(string roomId, Game game)
        {
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
                if (cardToRevive.ability != Abilities.zwinnośc)
                    abilityUseCases.AddCardToBoard(game, game.CurrentPlayer, cardToRevive.place, cardToRevive);

                await ExecuteCardAbility(roomId, game, cardToRevive);
                if (cardToRevive.ability != Abilities.wskrzeszenie && cardToRevive.ability != Abilities.zwinnośc)
                {
                    await FinishTurn(roomId, game);
                }
            }
        }

        public async Task ResolveDecoy(string roomId, int targetCardId)
        {
            if (!gameUseCases.games.TryGetValue(roomId, out var game)) return;

            var playerRows = game.CurrentPlayer == game.Player1
                ? new[] { game.Board.Player1FirstCardRow, game.Board.Player1SecondCardRow, game.Board.Player1ThirdCardRow }
                : new[] { game.Board.Player2FirstCardRow, game.Board.Player2SecondCardRow, game.Board.Player2ThirdCardRow };

            Card cardOnBoard = null;
            foreach (var row in playerRows)
            {
                cardOnBoard = row.FirstOrDefault(c => c.Id == targetCardId);
                if (cardOnBoard != null && !cardOnBoard.isChampion)
                {
                    abilityUseCases.ManekinDoCwiczenAbility(game, cardOnBoard);
                    await FinishTurn(roomId, game);
                    break;
                }
            }
        }

        private async Task ExecuteCardAbility(string roomId, Game game, Card card, int? selectedRow = null)
        {
            if (card.isCommander)
            {
                return;
            }

            switch (card.ability)
            {
                case Abilities.trzaskajacyMroz: abilityUseCases.TrzaskajacyMrozAbility(game); break;
                case Abilities.gestaMgla: abilityUseCases.GestaMglaAbility(game); break;
                case Abilities.ulewnyDeszcz: abilityUseCases.UlewnyDeszczAbility(game); break;
                case Abilities.czysteNiebo: abilityUseCases.CzysteNieboAbility(game); break;
                case Abilities.pozoga: abilityUseCases.Pozoga(game); break;
                case Abilities.manekinDoCwiczen:
                    await Clients.Caller.SendAsync("SelectCardToDecoy", card.Id);
                    break;
                case Abilities.wskrzeszenie:
                    if (HasRevivableCards(game))
                        await Clients.Caller.SendAsync("RequestResurrectionTarget");
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
                    // Dla pasywnych skilli jak Więź czy Morale
                    abilityUseCases.WiezAbility(game);
                    abilityUseCases.WyzszeMoraleAbility(game);
                    break;
            }
        }
    }
}
