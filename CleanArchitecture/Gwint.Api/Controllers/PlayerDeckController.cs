using Gwint.Api.Contracts;
using Gwint.Application.Decks;
using Gwint.Domain.Common;
using Microsoft.AspNetCore.Mvc;

namespace Gwint.Api.Controllers;

/// <summary>
/// Cienki kontroler budowania talii. Logika walidacji talii NIE jest już tutaj
/// (jak w poprzedniej wersji) - została przeniesiona do agregatu Deck.
/// Kontroler jedynie woła przypadki użycia DeckService.
/// </summary>
[ApiController]
[Route("api/player-deck")]
public sealed class PlayerDeckController : ControllerBase
{
    private readonly DeckService _decks;

    public PlayerDeckController(DeckService decks) => _decks = decks;

    [HttpGet("get-player-deck/{playerId:int}")]
    public async Task<IActionResult> GetPlayerDecks(int playerId, CancellationToken ct)
    {
        var deck = await _decks.GetPlayerDeckAsync(playerId, ct);
        return Ok(deck);
    }

    [HttpGet("get-player-fraction-deck/{playerId:int}/{fraction:int}")]
    public async Task<IActionResult> GetPlayerFractionDeck(int playerId, int fraction, CancellationToken ct)
    {
        try
        {
            var deck = await _decks.GetFractionDeckAsync(playerId, fraction, ct);
            return Ok(deck);
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("update-deck")]
    public async Task<IActionResult> UpdateDeck([FromBody] UpdateDeckRequest body, CancellationToken ct)
    {
        try
        {
            var command = new UpdateDeckCommand(
                body.PlayerId,
                body.Fraction,
                body.CardIdsToAdd,
                body.CardIdsToRemove);

            var updatedDeck = await _decks.UpdateDeckAsync(command, ct);
            return Ok(updatedDeck);
        }
        catch (DomainException ex)
        {
            // Reguły składu talii pilnuje agregat - tu tylko tłumaczymy je na 400.
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("available-cards/{fraction:int}")]
    public async Task<IActionResult> GetAvailableCards(int fraction, CancellationToken ct)
    {
        try
        {
            var cards = await _decks.GetAvailableCardsAsync(fraction, ct);
            return Ok(cards);
        }
        catch (DomainException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
