namespace Gwint.Application.Players;

/// <summary>Dane gracza zwracane na zewnątrz (bez hasła/hasha).</summary>
public sealed record PlayerDto(int Id, string Login);
