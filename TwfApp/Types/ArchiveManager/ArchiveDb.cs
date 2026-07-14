using Microsoft.Data.Sqlite;

namespace TwfApp.Types.ArchiveManager;

public class ArchiveDb
{
    private readonly SqliteConnection _dbConnection;

    public ArchiveDb(SqliteConnection dbConnection)
    {
        _dbConnection = dbConnection;
    }
}