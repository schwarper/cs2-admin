using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using Dapper;
using MySqlConnector;
using static Admin.Admin;

namespace Admin;

public static class Database
{
    private static string? GlobalDatabaseConnectionString;

    public static async Task<MySqlConnection> ConnectAsync()
    {
        MySqlConnection connection = new(GlobalDatabaseConnectionString);
        await connection.OpenAsync();
        return connection;
    }


    public static void ExecuteAsync(string query, object? parameters)
    {
        Task.Run(async () =>
        {
            using MySqlConnection connection = await ConnectAsync();
            await connection.ExecuteAsync(query, parameters);
        });
    }
    public static async Task CreateDatabaseAsync(AdminConfig config)
    {
        MySqlConnectionStringBuilder builder = new()
        {
            Server = config.Database["host"],
            Database = config.Database["name"],
            UserID = config.Database["user"],
            Password = config.Database["password"],
            Port = uint.Parse(config.Database["port"]),
            Pooling = true,
            MinimumPoolSize = 0,
            MaximumPoolSize = 640,
            ConnectionIdleTimeout = 30,
            AllowZeroDateTime = true
        };

        GlobalDatabaseConnectionString = builder.ConnectionString;

        using MySqlConnection connection = await ConnectAsync();
        using MySqlTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS baseban (
                        id INT NOT NULL AUTO_INCREMENT,
                        steamid BIGINT UNSIGNED NOT NULL,
                        duration INT NOT NULL,
                        end TIMESTAMP NOT NULL,
                        created TIMESTAMP NOT NULL,
                        PRIMARY KEY (id),
                        UNIQUE KEY id (id),
                        UNIQUE KEY stSteamIDstSteamID
                );
            ");

            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS baseban_log (
                        player_steamid BIGINT UNSIGNED NOT NULL,
                        player_name varchar(128),
                        admin_steamid BIGINT UNSIGNED,
                        admin_name varchar(128) NOT NULL,
                        command varchar(128) NOT NULL,
                        reason varchar(32),
                        duration INT,
                        date TIMESTAMP NOT NULL
                );
            ");

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public static async Task<bool> IsBanned(ulong steamid)
    {
        using MySqlConnection connection = await ConnectAsync();

        var results = await connection.QueryAsync("SELECT * FROM baseban WHERE steamid = @steamid;", new { steamid });

        return results.Any();
    }

    public static async Task Ban(CCSPlayerController player, CCSPlayerController? admin, string reason, int duration)
    {
        await Addban(player, player.SteamID, admin, reason, duration);
    }

    public static async Task Ban(ulong steamid, CCSPlayerController? admin, string reason, int duration)
    {
        await Addban(null, steamid, admin, reason, duration);
    }

    private static async Task Addban(CCSPlayerController? player, ulong steamid, CCSPlayerController? admin, string reason, int duration)
    {
        DateTime created = DateTime.Now;
        DateTime end = created.AddMinutes(duration);

        using MySqlConnection connection = await ConnectAsync();
        using MySqlTransaction transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync(@"
            INSERT INTO baseban (steamid, duration, end, created) 
            VALUES (@steamid, @duration, @end, @created) 
            ON DUPLICATE KEY UPDATE 
            steamid = VALUES(steamid), 
            duration = VALUES(duration), 
            end = VALUES(end), 
            created = VALUES(created);
        ", new
        {
            steamid,
            duration,
            end,
            created
        }, transaction: transaction);

        await connection.ExecuteAsync(@"
            INSERT INTO baseban_log (player_steamid, player_name, admin_steamid, admin_name, command, reason, duration, date)
            VALUES (@player_steamid, @player_name, @admin_steamid, @admin_name, 'ban', @reason, @duration, @date);
        ", new
        {
            player_steamid = player?.SteamID ?? steamid,
            @player_name = player?.PlayerName,
            @admin_steamid = admin?.SteamID,
            @admin_name = admin?.PlayerName ?? "Console",
            reason,
            duration,
            @date = created
        }, transaction: transaction);
    }

    public static async Task Unban(ulong steamid, CCSPlayerController? admin)
    {
        DateTime created = DateTime.Now;

        using MySqlConnection connection = await ConnectAsync();
        using MySqlTransaction transaction = await connection.BeginTransactionAsync();

        await connection.ExecuteAsync("DELETE FROM baseban WHERE steamid = @steamid;", new { steamid }, transaction: transaction);

        await connection.ExecuteAsync(@"
            INSERT INTO baseban_log (player_steamid, admin_steamid, admin_name, command, date)
            VALUES (@player_steamid, @admin_steamid, @admin_name, 'unban', @date);
        ", new
        {
            player_steamid = steamid,
            @admin_steamid = admin?.SteamID ?? 0,
            @admin_name = admin?.PlayerName ?? "Console",
            @date = created
        }, transaction: transaction);
    }
}
