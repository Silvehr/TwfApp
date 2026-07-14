using Microsoft.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using System.Text.Json;
using TwfApp.Types.Config;
using TwfApp.Types.Results;
using System.Data.Common;

namespace TwfApp.Types
{
    public class JokeObjectDb
    {
        public string DbFilePath { get; }
        private readonly SqliteConnection _db;
        private readonly AppConfig _config;

        public JokeObjectDb(SqliteConnection db, AppConfig config)
        {
            _db = db;
            _config = config;
            DbFilePath = Path.Join(config.AppDataDirectory, $"{config.JokesDatabaseName}.db");
        }

        /// <summary>
        /// Emergency export of tables JokeObjects and DataVersion to JSON or SQL.
        /// Tries to work regardless of exact schema: reads column names dynamically.
        /// </summary>
        /// <param name="asSql">If true returns SQL INSERT statements, otherwise JSON.</param>

        public bool IsInitialized { get; private set; } = false;

        public async Task<Exception?> Initialize()
        {
            if (IsInitialized) return null;

            await _db.OpenAsync();

            try
            {
                using var command = _db.CreateCommand();
                using var stream = await FileSystem.OpenAppPackageFileAsync("Migrations/_snapshot.sql");
                using var reader = new StreamReader(stream);

                command.CommandText = await reader.ReadToEndAsync();

                await command.ExecuteNonQueryAsync();

                IsInitialized = true;
                return null;
            }
            catch(Exception e)
            {
                return e;
            }
        }

        public Task Stop()
        {
            IsInitialized = false;
            return Task.CompletedTask;
        }

        public async Task<Result<int, DbException>> CountJokeObjectsAsync()
        {
            if (!IsInitialized) return new(0);

            var command = _db.CreateCommand();
            command.CommandText = "SELECT COUNT(*) FROM JokeObjects;";

            try
            {
                var result = await command.ExecuteScalarAsync();
                return new(Convert.ToInt32(result));
            }
            catch(DbException e)
            {
                return new(e);
            }
        }

        public async Task<DbException?> AppendRangeAsync(IEnumerable<JokeObjectDto> items, bool editable = true)
        {
            // build VALUES string safely (escape single quotes)
            var valueString = string.Join(",", items.Select(n =>
                $"('{Escape(n.Author)}','{Escape(n.Name)}',{(n.Description is null ? "NULL" : $"'{Escape(n.Description)}'")}, {(int)n.Type},'{n.CreationDate:yyyy-MM-dd HH:mm:ss}',{(editable ? 1 : 0)})"));

            var insertCommand = _db.CreateCommand();
            insertCommand.CommandText = $"INSERT INTO JokeObjects (Author, Name, Description, Type, CreationDate, Editable) VALUES {valueString} ON CONFLICT(Name) DO NOTHING;";
            try
            {
                await insertCommand.ExecuteNonQueryAsync();
                return null;
            }
            catch(DbException e)
            {
                return e;
            }
        }

        private static string Escape(string s) => s?.Replace("'", "''") ?? string.Empty;

        public async Task<Result<bool, DbException>> ExistsWithName(string name)
        {
            using var command = _db.CreateCommand();
            command.CommandText = "SELECT EXISTS(SELECT 1 FROM JokeObjects WHERE Name = @Name);";
            command.Parameters.AddWithValue("@Name", name);
            object? scalar;
            try
            {
                scalar = await command.ExecuteScalarAsync();
            }
            catch(DbException e)
            {
                return new(e);
            }

            if (scalar is null || scalar is DBNull) return new(false);
            if (scalar is long l) return new(l != 0);
            if (scalar is int i) return new(i != 0);
            if (scalar is bool b) return new(b);

            try
            {
                return new(Convert.ToBoolean(scalar));
            }
            catch
            {
                return new(scalar.ToString() == "1");
            }
        }

        public async Task<Result<JokeObject?, DbException>> GetByIdAsync(int id)
        {
            using var command = _db.CreateCommand();
            command.CommandText = "SELECT Id, Author, Name, Description, Type, CreationDate, Editable FROM JokeObjects WHERE Id = @Id;";
            command.Parameters.AddWithValue("@Id", id);
            SqliteDataReader? reader;
            JokeObject? result = null;
            try
            {
                reader = await command.ExecuteReaderAsync();
            }
            catch (DbException e) 
            {
                return new(e);
            }

            if (await reader.ReadAsync())
                result = new JokeObject(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.IsDBNull(3)
                        ? null
                        : reader.GetString(3),
                    (JokeObjectType)reader.GetInt32(4),
                    reader.GetDateTime(5),
                    reader.GetBoolean(6));

            await reader.DisposeAsync();
            return new(result);
        }

        public async Task<Result<IEnumerable<JokeObject>, DbException>> GetByAuthorAsync(string authorName)
        {
            using var command = _db.CreateCommand();
            command.CommandText = "SELECT Id, Author, Name, Description, Type, CreationDate, Editable FROM JokeObjects WHERE Author = @Author;";
            command.Parameters.AddWithValue("@Author", authorName);

            SqliteDataReader? reader;

            try
            {
                reader = await command.ExecuteReaderAsync();
            }
            catch(DbException e)
            {
                return new(e);
            }

            var jokeObjects = new List<JokeObject>();
            while (await reader.ReadAsync())
            {
                var desc = reader.IsDBNull(3) ? null : reader.GetString(3);
                jokeObjects.Add(new JokeObject(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), desc, (JokeObjectType)reader.GetInt32(4), reader.GetDateTime(5), reader.GetBoolean(6)));
            }

            return new(jokeObjects);
        }

        /// <summary>
        /// Creates me JokeObject and adds it to the database
        /// </summary>
        /// <param name="author">Author's nickname</param>
        /// <param name="name">Name of the object</param>
        /// <param name="description">Description of the object. History of it's creation etc.</param>
        /// <param name="type">Where in the joke should it appear</param>
        /// <param name="creationDate">When it first appeared</param>
        /// <returns>
        /// <para>
        /// <see cref="DbException" /> if database error occouts
        /// </para>
        /// <para>
        /// <see cref="InvalidOperationException" /> if object with same name occouts
        /// </para>
        /// <para>
        /// <see cref="JokeObject" /> if none of the above happens
        /// </para>
        /// </returns>
        public async Task<Result<JokeObject?, Exception>> CreateAsync(string author, string name, string? description, JokeObjectType type, DateTime creationDate)
        {
            var existWithNameResult = await ExistsWithName(name);

            if (!existWithNameResult.IsSuccess)
                return new(existWithNameResult.Error!);

            if (existWithNameResult.Value)
                return new(new InvalidOperationException());

            using var insertCmd = _db.CreateCommand();
            insertCmd.CommandText = "INSERT INTO JokeObjects (Author, Name, Description, Type, CreationDate) VALUES (@Author, @Name, @Description, @Type, @CreationDate);";
            insertCmd.Parameters.AddWithValue("@Author", author);
            insertCmd.Parameters.AddWithValue("@Name", name);
            insertCmd.Parameters.AddWithValue("@Description", description ?? (object)DBNull.Value);
            insertCmd.Parameters.AddWithValue("@Type", (int)type);
            insertCmd.Parameters.AddWithValue("@CreationDate", creationDate);
            try
            {
                await insertCmd.ExecuteNonQueryAsync();
            }
            catch(DbException e)
            {
                return new(e);
            }

            using var idCmd = _db.CreateCommand();
            idCmd.CommandText = "SELECT last_insert_rowid();";
            object? scalar;

            try
            {
                scalar = await idCmd.ExecuteScalarAsync();
            }
            catch (DbException e) {
                return new Result<JokeObject?, Exception>(e);
            }

            if (scalar is null || scalar is DBNull) return new(value: null);

            long id64;
            if (scalar is long l)
                id64 = l;
            else
                id64 = Convert.ToInt64(scalar);

            var id = Convert.ToInt32(id64);
            return new(new JokeObject(id, author, name, description, type, creationDate));
        }

        public async Task<DbException?> EditAsync(JokeObject jokeObject)
        {
            using var command = _db.CreateCommand();
            command.CommandText = "UPDATE JokeObjects SET Author = @NewAuthor, Name = @NewName, Description = @NewDescription, Type = @NewType,CreationDate = @NewCreationDate WHERE Id = @Id AND Editable = 1;";

            command.Parameters.AddWithValue("@Id", jokeObject.Id);
            command.Parameters.AddWithValue("@NewAuthor", jokeObject.Author);
            command.Parameters.AddWithValue("@NewName", jokeObject.Name);
            command.Parameters.AddWithValue("@NewDescription", jokeObject.Description ?? (object)DBNull.Value);
            command.Parameters.AddWithValue("@NewType", (int)jokeObject.Type);
            command.Parameters.AddWithValue("@NewCreationDate", jokeObject.CreationDate);

            try
            {
                await command.ExecuteNonQueryAsync();
                return null;
            }
            catch(DbException e)
            {
                return e;
            }
        }

        public async Task<DbException?> RemoveAsync(int id)
        {
            using var command = _db.CreateCommand();
            command.CommandText = "DELETE FROM JokeObjects WHERE Id = @Id AND Editable = 1;";
            command.Parameters.AddWithValue("@Id", id);
            try
            {
                await command.ExecuteNonQueryAsync();
                return null;
            }
            catch(DbException e)
            {
                return e;
            }
        }

        public async Task<Result<IEnumerable<JokeObject>, Exception>> GetJokeObjectsAsync(string namePart = "", int count = -1)
        {
            var jokeObjects = new List<JokeObject>();
            using var command = _db.CreateCommand();
            command.CommandText = "SELECT Id, Author, Name, Description, Type, CreationDate, Editable FROM JokeObjects WHERE Name LIKE @NamePart";
            command.Parameters.AddWithValue("@NamePart", $"%{namePart}%");
            if (count > 0)
            {
                command.CommandText += " LIMIT @Count";
                command.Parameters.AddWithValue("@Count", count);
            }
            SqliteDataReader? reader;
            try
            {
                reader = await command.ExecuteReaderAsync();
            }
            catch (DbException e) {
                return new(e);
            }

            while (await reader.ReadAsync())
            {
                try
                {
                    var id = reader.GetInt32(0);
                    var author = reader.GetString(1);
                    var name = reader.GetString(2);
                    string? desc = reader.IsDBNull(3) ? null : reader.GetString(3);
                    var type = (JokeObjectType)reader.GetInt32(4);
                    var creation = reader.GetDateTime(5);
                    var editable = reader.GetBoolean(6);
                    jokeObjects.Add(new JokeObject(id, author, name, desc, type, creation, editable));
                }
                catch(Exception e)
                {
                    return new(e);
                }
            }

            return new(jokeObjects);
        }

        public async Task<Result<JokeObject[], Exception>> GetRandomObjectsByCategory(JokeObjectType searchedType, int count)
        {
            var jokeObjects = new JokeObject[count];

            using var command = _db.CreateCommand();
            command.CommandText = "SELECT Id, Author, Name, Description, Type, CreationDate, Editable FROM JokeObjects WHERE Type = @SearchedType ORDER BY RANDOM() LIMIT @Count;";
            command.Parameters.AddWithValue("@Count", count);
            command.Parameters.AddWithValue("@SearchedType", (int)searchedType);

            SqliteDataReader? reader;
            try
            {
                reader = await command.ExecuteReaderAsync();
            }
            catch(DbException e)
            {
                return new(error: e);
            }

            int index = 0;
            while (await reader.ReadAsync())
            {
                try
                {
                    var id = reader.GetInt32(0);
                    var author = reader.GetString(1);
                    var name = reader.GetString(2);
                    string? desc = reader.IsDBNull(3) ? null : reader.GetString(3);
                    var type = (JokeObjectType)reader.GetInt32(4);
                    var creation = reader.GetDateTime(5);
                    var editable = reader.GetBoolean(6);
                    jokeObjects[index++] = new JokeObject(id, author, name, desc, type, creation, editable);
                }
                catch(Exception e)
                {
                    return new(error: e);
                }
            }

            return new(value: jokeObjects);
        }


        public async Task<Result<string, Exception>> EmergencyExport(bool asSql = false)
        {
            try
            {
                // ensure connection
                if (_db.State != System.Data.ConnectionState.Open)
                    await _db.OpenAsync();

                var tables = new[] { "JokeObjects", "DataVersion" };
                var export = new Dictionary<string, object>();

                foreach (var table in tables)
                {
                    // check table exists
                    using var chk = _db.CreateCommand();
                    chk.CommandText = "SELECT name FROM sqlite_master WHERE type='table' AND name=@name;";
                    chk.Parameters.AddWithValue("@name", table);
                    var exists = await chk.ExecuteScalarAsync();
                    if (exists is null) continue;

                    // get columns
                    using var colCmd = _db.CreateCommand();
                    colCmd.CommandText = $"PRAGMA table_info('{table}');";
                    var cols = new List<string>();
                    using (var reader = await colCmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            cols.Add(reader.GetString(1)); // name
                        }
                    }

                    if (asSql)
                    {
                        // build INSERTs
                        var sb = new StringBuilder();
                        using var sel = _db.CreateCommand();
                        sel.CommandText = $"SELECT * FROM '{table}';";
                        using var r = await sel.ExecuteReaderAsync();
                        while (await r.ReadAsync())
                        {
                            var colList = string.Join(", ", cols.Select(c => $"\"{c}\""));
                            var values = new List<string>();
                            for (int i = 0; i < cols.Count; i++)
                            {
                                if (r.IsDBNull(i)) { values.Add("NULL"); continue; }
                                var val = r.GetValue(i);
                                if (val is string s)
                                {
                                    values.Add($"'{s.Replace("'", "''")}'");
                                }
                                else if (val is byte[] b)
                                {
                                    values.Add($"X'" + BitConverter.ToString(b).Replace("-", "") + "'");
                                }
                                else if (val is bool bb)
                                {
                                    values.Add(bb ? "1" : "0");
                                }
                                else
                                {
                                    values.Add(Convert.ToString(val, System.Globalization.CultureInfo.InvariantCulture));
                                }
                            }
                            sb.AppendLine($"INSERT INTO \"{table}\" ({colList}) VALUES ({string.Join(", ", values)});");
                        }

                        export[table] = sb.ToString();
                    }
                    else
                    {
                        var rows = new List<Dictionary<string, object?>>();
                        using var sel = _db.CreateCommand();
                        sel.CommandText = $"SELECT * FROM '{table}';";
                        using var r = await sel.ExecuteReaderAsync();
                        while (await r.ReadAsync())
                        {
                            var row = new Dictionary<string, object?>();
                            for (int i = 0; i < cols.Count; i++)
                            {
                                if (r.IsDBNull(i)) { row[cols[i]] = null; continue; }
                                var val = r.GetValue(i);
                                if (val is byte[] b)
                                {
                                    row[cols[i]] = Convert.ToBase64String(b);
                                }
                                else
                                {
                                    // keep raw types (int64, double, string, etc.)
                                    row[cols[i]] = val;
                                }
                            }
                            rows.Add(row);
                        }

                        export[table] = rows;
                    }
                }

                if (asSql)
                {
                    // join SQL for both tables
                    var combined = new StringBuilder();
                    foreach (var t in export)
                    {
                        combined.AppendLine($"-- Table: {t.Key}");
                        combined.AppendLine(t.Value as string ?? string.Empty);
                    }
                    return new(combined.ToString());
                }

                var options = new JsonSerializerOptions { WriteIndented = true };
                var json = JsonSerializer.Serialize(export, options);
                return new(json);
            }
            catch (Exception ex)
            {
                return new(ex);
            }
        }
    }
}
