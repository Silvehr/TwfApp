namespace TwfApp.Types.ArchiveManager;

public enum ArchiveEntryStatus
{
    Alive = 0,
    Niche = 1,
    EdgeOfLifeAndDeath = 2,
    Dead = 3
}
public record ArchiveEntry(int Id, string Author, string Name, string? Description, ArchiveEntryStatus Status, DateTime CreationDate, DateTime LastModifiedDate);