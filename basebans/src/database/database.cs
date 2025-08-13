using System.Data.Common;
using Dapper;
using MySqlConnector;

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
        await connection.ExecuteAsync(CreateBasebanTableSql);
    }

    public static bool IsBanned(ulong steamid)
    {
        return IsBannedAsync(steamid).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static async Task<bool> IsBannedAsync(ulong steamid)
    {
        await using DbConnection connection = await ConnectAsync().ConfigureAwait(false);

        await connection.ExecuteAsync(UpdateExpiredBansSql).ConfigureAwait(false);

        bool exists = await connection.ExecuteScalarAsync<bool>(SelectBansSql, new { steamid }).ConfigureAwait(false);
        return exists;
    }

    public static async Task Ban(string playername, ulong steamid, string adminname, ulong adminsteamid, string reason, int duration)
    {
        DateTime created = DateTime.Now;
        DateTime end = duration == 0 ? DateTime.MaxValue : created.AddMinutes(duration);

        await using DbConnection connection = await ConnectAsync();

        await connection.ExecuteAsync(InsertBasebanSql, new
        {
            steamid,
            playername,
            adminsteamid,
            adminname,
            reason,
            duration,
            created,
            end,
            status = "ACTIVE"
        });
    }

    public static async Task Unban(ulong steamid)
    {
        await using DbConnection connection = await ConnectAsync();
        await connection.ExecuteAsync(UpdateStatusBasebanSql, new { steamid, status = "UNBANNED" });
    }

    private const string CreateBasebanTableSql = @"
        CREATE TABLE IF NOT EXISTS baseban (
            id INT AUTO_INCREMENT PRIMARY KEY,
            steamid BIGINT UNSIGNED NOT NULL,
            playername VARCHAR(128) NOT NULL,
            adminsteamid BIGINT UNSIGNED NOT NULL,
            adminname VARCHAR(128) NOT NULL,
            reason VARCHAR(128),
            duration INT NOT NULL,
            created DATETIME DEFAULT CURRENT_TIMESTAMP NOT NULL,
            end DateTime NOT NULL,
            status ENUM('ACTIVE', 'UNBANNED', 'EXPIRED') DEFAULT 'ACTIVE' NOT NULL
        );
    ";

    private const string InsertBasebanSql = @"
        INSERT INTO baseban (steamid, playername, adminsteamid, adminname, reason, duration, created, end, status) 
        VALUES (@steamid, @playername, @adminsteamid, @adminname, @reason, @duration, @created, @end, @status);
    ";

    private const string UpdateStatusBasebanSql = @"
        UPDATE baseban 
        SET status = @status 
        WHERE steamid = @steamid;
    ";

    private const string UpdateExpiredBansSql = @"
        UPDATE baseban 
        SET status = 'EXPIRED' 
        WHERE end <= NOW() AND status = 'ACTIVE';
    ";

    private const string SelectBansSql = @"
        SELECT EXISTS(SELECT 1 FROM baseban WHERE steamid = @steamid AND status = 'ACTIVE' LIMIT 1);
    ";
}