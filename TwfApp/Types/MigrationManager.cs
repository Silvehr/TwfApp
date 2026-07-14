using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Text;

namespace TwfApp.Types
{
    public class MigrationManager
    {
        private readonly SqliteConnection _dbConnection;

        public MigrationManager(SqliteConnection connection)
        {
            _dbConnection = connection;
        }
    }
}
