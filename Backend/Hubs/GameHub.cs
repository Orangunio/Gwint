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
        public GameHub(GameUseCases gameUseCases, GwintDBContext gwintDBContext) 
        { 
            this.gameUseCases = gameUseCases;
            this.gwintDBContext = gwintDBContext;
        }

        public async Task StartGame(string roomId, Fractions player1SelectedFraction, Fractions player2SelectedFraction)
        {
            if (RoomHub.rooms.TryGetValue(roomId, out var room) && room.Players.Count == 2)
            {
                var player1Id = room.Players[0].Id;
                var player2Id = room.Players[1].Id;

                var player1Deck = await gwintDBContext.PlayerDecks
                    .Where(pd => pd.PlayerId == player1Id)
                    .Include(pd => pd.Card)
                    .Where(pd => pd.Card.fraction == player1SelectedFraction)
                    .Select(pd => pd.Card)
                    .Take(20)
                    .ToListAsync();

                var player2Deck = await gwintDBContext.PlayerDecks
                    .Where(pd => pd.PlayerId == player2Id)
                    .Include(pd => pd.Card)
                    .Where(pd => pd.Card.fraction == player2SelectedFraction)
                    .Select(pd => pd.Card)
                    .Take(20)
                    .ToListAsync();

                var game = new Game(player1Deck, player2Deck, player1SelectedFraction, player2SelectedFraction);

                game.Player1 = room.Players[0];
                game.Player2 = room.Players[1];

                game.RoomId = roomId;

                //Przypisuje karty dowódców
                game.Player1CommanderCard = player1Deck.FirstOrDefault(c => c.isCommander);
                game.Player2CommanderCard = player2Deck.FirstOrDefault(c => c.isCommander);
                player1Deck.Remove(game.Player1CommanderCard);
                player2Deck.Remove(game.Player2CommanderCard);

                //Przypisuje karty do talii
                game.Player1CardInDeck = GameUseCases.Shuffle(player1Deck);
                game.Player2CardInDeck = GameUseCases.Shuffle(player2Deck);

                // rozdanie kart
                game.Player1CardsOnHand = game.Player1CardInDeck.Take(10).ToList();
                game.Player2CardsOnHand = game.Player2CardInDeck.Take(10).ToList();
                game.Player1CardInDeck = game.Player1CardInDeck.Skip(10).ToList();
                game.Player2CardInDeck = game.Player2CardInDeck.Skip(10).ToList();

                gameUseCases.games.TryAdd(roomId, game);

                await Clients.Group(roomId).SendAsync("GameStarted", game);
            }
        }

        public async Task SetFirstPlayer(string roomId, string chosenConnectionId)
        {
            var player = gameUseCases.SetFirstPlayer(roomId, chosenConnectionId);

            await Clients.Group(roomId)
                .SendAsync("TurnStarted", player.ConnectionId);
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

        public async Task PlayCard(string roomId, Card selectedCardByUser)
        {
            var game = gameUseCases.games[roomId];

            if (selectedCardByUser.isSpecial)
            {
                //Rozpatrzyc efekt w zaleznosci od card.ability
            }
            else if (selectedCardByUser.isCommander)
            {
                //Rozpatrzyc efekt w zaleznosci od card.ability

                //Usun karte z gry
            }
            else
            {
                //Przypisac jednostke do odpowiedniego rzedu (Uwaga Szpieg)

                //Rozpatrzec umiejetnosc jednostki jezeli taka posiada
            }

            if(game.CurrentPlayer == game.Player1)
            {
                if (game.Player2Passed)
                {
                    //Ponownie rusza się gracz 1
                }
                else
                {
                    game.CurrentPlayer = game.Player2;
                }
            }
            else
            {
                if (game.Player1Passed)
                {
                    //Ponownie rusza się gracz 2
                }
                else
                {
                    game.CurrentPlayer = game.Player1;
                }
            }

            await Clients.Group(roomId)
                    .SendAsync("NextTurn", game.CurrentPlayer.ConnectionId);
        }
    }
}
