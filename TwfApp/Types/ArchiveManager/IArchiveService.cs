namespace TwfApp.Types.ArchiveManager;

public interface IArchiveService
{
    Task<ArchiveEntry?> CreateAsync(string author, string name, string? description, ArchiveEntryStatus status, DateTime creationDate);
    Task<ArchiveEntry?> GetByIdAsync(int id);
    Task<IEnumerable<ArchiveEntry>> GetByNameAsync(string name);
    
}