using Dapper;
using MySqlConnector;
using System.Data.Common;

namespace BaseBans;

public static class Database
{
    private static string DatabaseConnectionString { get; set; } = string.Empty;

    private static async Task<MySqlConnection> ConnectAsync()
    {
        MySqlConnection connection = new(DatabaseConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public static async Task CreateDatabaseAsync(Config config)
    {
        if (string.IsNullOrEmpty(config.Database.Host) || string.IsNullOrEmpty(config.Database.Name) || string.IsNullOrEmpty(config.Database.User))
        {
            throw new Exception("You need to setup Database credentials in config.");
        }

        MySqlConnectionStringBuilder builder = new()
        {
            Server = config.Database.Host,
            Database = config.Database.Name,
            UserID = config.Database.User,
            Password = config.Database.Password,
            Port = config.Database.Port,
            Pooling = true,
            MinimumPoolSize = 0,
            MaximumPoolSize = 600,
            ConnectionIdleTimeout = 30,
            AllowZeroDateTime = true
        };

        DatabaseConnectionString = builder.ConnectionString;

        await using DbConnection connection = await ConnectAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            await connection.ExecuteAsync(CreateBasebanTableSql, transaction: transaction);
            await connection.ExecuteAsync(CreateBasebanLogTableSql, transaction: transaction);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw new Exception(ex.Message);
        }
    }

    public static bool IsBanned(ulong steamid)
    {
        return IsBannedAsync(steamid).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static async Task<bool> IsBannedAsync(ulong steamid)
    {
        await using DbConnection connection = await ConnectAsync().ConfigureAwait(false);
        bool exists = await connection.ExecuteScalarAsync<bool>(DeleteAndSelectSql, new { steamid }).ConfigureAwait(false);
        return exists;
    }

    public static async Task Ban(string playername, ulong playersteamid, string adminname, ulong adminsteamid, string reason, int duration)
    {
        DateTime created = DateTime.Now;
        DateTime end = duration == 0 ? DateTime.MaxValue : created.AddMinutes(duration);

        await using DbConnection connection = await ConnectAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            await connection.ExecuteAsync(InsertBasebanSql, new
            {
                steamid = playersteamid,
                duration,
                created,
                end
            }, transaction: transaction);

            await connection.ExecuteAsync(InsertBasebanLogSql, new
            {
                player_steamid = playersteamid,
                player_name = playername,
                admin_steamid = adminsteamid,
                admin_name = adminname,
                command = "ban",
                reason,
                duration,
                date = created
            }, transaction: transaction);

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public static async Task Unban(string playername, ulong steamid, string adminname, ulong adminsteamid)
    {
        DateTime created = DateTime.Now;

        await using DbConnection connection = await ConnectAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            await connection.ExecuteAsync(DeleteBasebanSql, new { steamid }, transaction: transaction);

            await connection.ExecuteAsync(InsertBasebanLogSql, new
            {
                player_steamid = steamid,
                player_name = playername,
                admin_steamid = adminsteamid,
                admin_name = adminname,
                command = "unban",
                reason = string.Empty,
                duration = 0,
                date = created
            }, transaction: transaction);

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private const string CreateBasebanTableSql = @"
            CREATE TABLE IF NOT EXISTS baseban (
                id INT AUTO_INCREMENT PRIMARY KEY,
                steamid BIGINT UNSIGNED NOT NULL,
                duration INT NOT NULL,
                created DateTime NOT NULL,
                end DateTime NOT NULL,
                UNIQUE (steamid)
            );
        ";

    private const string CreateBasebanLogTableSql = @"
            CREATE TABLE IF NOT EXISTS baseban_log (
                player_steamid BIGINT UNSIGNED NOT NULL,
                player_name VARCHAR(128),
                admin_steamid BIGINT UNSIGNED,
                admin_name VARCHAR(128) NOT NULL,
                command VARCHAR(128) NOT NULL,
                reason VARCHAR(128),
                duration INT,
                date DateTime NOT NULL
            );
        ";

    private const string InsertBasebanSql = @"
        INSERT INTO baseban (steamid, duration, created, end) 
        VALUES (@steamid, @duration, @created, @end);
    ";

    private const string InsertBasebanLogSql = @"
        INSERT INTO baseban_log (player_steamid, player_name, admin_steamid, admin_name, command, reason, duration, date)
        VALUES (@player_steamid, @player_name, @admin_steamid, @admin_name, @command, @reason, @duration, @date);
    ";

    private const string DeleteBasebanSql = "DELETE FROM baseban WHERE steamid = @steamid;";

    private const string DeleteAndSelectSql = @"
        DELETE FROM baseban 
        WHERE steamid = @steamid AND end <= NOW();
    
        SELECT EXISTS(SELECT 1 FROM baseban WHERE steamid = @steamid LIMIT 1);
    ";
}