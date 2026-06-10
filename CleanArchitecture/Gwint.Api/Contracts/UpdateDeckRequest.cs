namespace Gwint.Api.Contracts;

/// <summary>Body żądania aktualizacji talii (kontrakt frontendu).</summary>
public sealed class UpdateDeckRequest
{
    public int PlayerId { get; set; }
    public int Fraction { get; set; }
    public List<int> CardIdsToAdd { get; set; } = new();
    public List<int> CardIdsToRemove { get; set; } = new();
}
