namespace Gwint.Infrastructure.Persistence.Records;

internal sealed class PlayerRecord
{
    public int Id { get; set; }
    public string Login { get; set; } = string.Empty;
    public string HashPassword { get; set; } = string.Empty;
}

internal sealed class CardRecord
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int fraction { get; set; }
    public int ability { get; set; }
    public int Strength { get; set; }
    public int place { get; set; }
    public bool isChampion { get; set; }
    public bool isCommander { get; set; }
    public bool isSpecial { get; set; }
}

internal sealed class PlayerDeckRecord
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int CardId { get; set; }
}
