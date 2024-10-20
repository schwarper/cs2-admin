using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dapper;
using MySqlConnector;
using System.Data.Common;
using static BaseCommTemp.BaseCommTemp;

namespace BaseCommTemp;

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
        await connection.ExecuteAsync(CreateBasecommtempTableSql);
    }

    public static bool IsPunished(ulong steamid, string status)
    {
        return IsPunishedAsync(steamid, status).ConfigureAwait(false).GetAwaiter().GetResult();
    }

    public static async Task<bool> IsPunishedAsync(ulong steamid, string status)
    {
        await using DbConnection connection = await ConnectAsync().ConfigureAwait(false);

        bool exists = await connection.ExecuteScalarAsync<bool>(SelectCommSql, new { steamid, status }).ConfigureAwait(false);
        return exists;
    }

    public static async Task UpdatePunishs()
    {
        await using DbConnection connection = await ConnectAsync();

        await connection.ExecuteAsync(UpdateExpiredCommSql);
    }

    public static async Task LoadPlayer(ulong steamid)
    {
        await using MySqlConnection connection = await ConnectAsync();

        IEnumerable<dynamic> results = await connection.QueryAsync(SelectCommsSql, new { steamid });

        foreach (var result in results)
        {
            PlayerTemporaryPunishList.Add(new PunishInfo()
            {
                SteamID = steamid,
                PunishName = result.status,
                Duration = result.duration,
                Created = ((MySqlDateTime)result.created).GetDateTime(),
                End = ((MySqlDateTime)result.end).GetDateTime()
            });
        }
    }

    public static async Task Punish(string playername, ulong steamid, string adminname, ulong adminsteamid, string reason, int duration, string status)
    {
        DateTime created = DateTime.Now;
        DateTime end = created.AddMinutes(duration);

        await using MySqlConnection connection = await ConnectAsync();

        await connection.ExecuteAsync(InsertBasecommtempSql, new
        {
            steamid,
            playername,
            adminsteamid,
            adminname,
            reason,
            duration,
            created,
            end,
            status
        });
    }

    public static async Task UnPunish(ulong steamid, string oldstatus, string newstatus)
    {
        await using DbConnection connection = await ConnectAsync();
        await connection.ExecuteAsync(UpdateStatusBasecommtempSql, new { steamid, newstatus, oldstatus });
    }


    private const string CreateBasecommtempTableSql = @"
        CREATE TABLE IF NOT EXISTS basecommtemp (
            id INT AUTO_INCREMENT PRIMARY KEY,
            steamid BIGINT UNSIGNED NOT NULL,
            playername VARCHAR(128) NOT NULL,
            adminsteamid BIGINT UNSIGNED NOT NULL,
            adminname VARCHAR(128) NOT NULL,
            reason VARCHAR(128),
            duration INT NOT NULL,
            created DateTime NOT NULL,
            end DateTime NOT NULL,
            status ENUM('GAG', 'MUTE', 'UNGAGGED', 'UNMUTED', 'EXPIRED') NOT NULL
        );
    ";

    private const string InsertBasecommtempSql = @"
        INSERT INTO basecommtemp (steamid, playername, adminsteamid, adminname, reason, duration, created, end, status) 
        VALUES (@steamid, @playername, @adminsteamid, @adminname, @reason, @duration, @created, @end, @status);
    ";

    private const string UpdateStatusBasecommtempSql = @"
        UPDATE basecommtemp
        SET status = @newstatus
        WHERE steamid = @steamid AND status = @oldstatus;
    ";

    private const string UpdateExpiredCommSql = @"
        UPDATE basecommtemp
        SET status = 'EXPIRED'
        WHERE end <= NOW() AND status = 'GAG' or status = 'MUTE';
    ";

    private const string SelectCommSql = @"
        SELECT EXISTS(SELECT 1 FROM basecommtemp WHERE steamid = @steamid AND status = @status LIMIT 1);
    ";

    private const string SelectCommsSql = @"
        SELECT * FROM basecommtemp WHERE steamid = @steamid AND status = 'GAG' or status = 'MUTE';
    ";
}