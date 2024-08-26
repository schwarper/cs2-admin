using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dapper;
using Microsoft.Data.Sqlite;
using MySqlConnector;
using System.Data.Common;
using static Admin.Admin;

namespace Admin;

public static class Database
{
    private static string FileName = string.Empty;
    private static string GlobalDatabaseConnectionString = string.Empty;

    private static async Task<DbConnection> ConnectAsync()
    {
        SQLitePCL.Batteries.Init();

        DbConnection connection = string.IsNullOrEmpty(GlobalDatabaseConnectionString) ?
            new SqliteConnection($"Data Source={FileName}") :
            new MySqlConnection(GlobalDatabaseConnectionString);

        await connection.OpenAsync();
        return connection;
    }

    public static void SetFileName(string filename)
    {
        FileName = filename;
    }

    public static async Task CreateDatabaseAsync(AdminConfig config)
    {
        if (config.Database.UseMySql)
        {
            MySqlConnectionStringBuilder builder = new()
            {
                Server = config.Database.Host,
                Database = config.Database.Name,
                UserID = config.Database.User,
                Password = config.Database.Password,
                Port = config.Database.Port,
                Pooling = true,
                MinimumPoolSize = 0,
                MaximumPoolSize = 640,
                ConnectionIdleTimeout = 30,
                AllowZeroDateTime = true
            };

            GlobalDatabaseConnectionString = builder.ConnectionString;
        }

        using DbConnection connection = await ConnectAsync();
        using DbTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            await CreateTableAsync(connection, transaction, config.Database.UseMySql);
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    private static async Task CreateTableAsync(DbConnection connection, DbTransaction transaction, bool useMySql)
    {
        string createBasebanTableSql = useMySql ?
            @"
                CREATE TABLE IF NOT EXISTS baseban (
                    id INT NOT NULL AUTO_INCREMENT,
                    steamid BIGINT UNSIGNED NOT NULL,
                    duration INT NOT NULL,
                    created DATETIME NOT NULL,
                    end DATETIME NOT NULL,
                    PRIMARY KEY (id),
                    UNIQUE KEY steamid (steamid)
                );
            "
            :
            @"
                CREATE TABLE IF NOT EXISTS baseban (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    steamid INTEGER NOT NULL,
                    duration INTEGER NOT NULL,
                    created DATETIME NOT NULL,
                    end DATETIME NOT NULL,
                    UNIQUE (steamid)
                );
        ";

        string createBasebanLogTableSql = useMySql ?
            @"
                CREATE TABLE IF NOT EXISTS baseban_log (
                    player_steamid BIGINT UNSIGNED NOT NULL,
                    player_name varchar(128),
                    admin_steamid BIGINT UNSIGNED,
                    admin_name varchar(128) NOT NULL,
                    command varchar(128) NOT NULL,
                    reason varchar(32),
                    duration INT,
                    date DATETIME NOT NULL
                );
            "
            :
            @"
                CREATE TABLE IF NOT EXISTS baseban_log (
                    player_steamid INTEGER NOT NULL,
                    player_name TEXT,
                    admin_steamid INTEGER,
                    admin_name TEXT NOT NULL,
                    command TEXT NOT NULL,
                    reason TEXT,
                    duration INTEGER,
                    date DATETIME NOT NULL
                );
        ";

        string createBasecommTableSql = useMySql ?
            @"
                CREATE TABLE IF NOT EXISTS basecomm (
                    id INT NOT NULL AUTO_INCREMENT,
                    steamid BIGINT UNSIGNED NOT NULL,
                    command varchar(128) NOT NULL,
                    duration INT NOT NULL,
                    created DATETIME NOT NULL,
                    end DATETIME NOT NULL,
                    PRIMARY KEY (id)
                );
            "
            :
            @"
                CREATE TABLE IF NOT EXISTS basecomm (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    steamid INTEGER NOT NULL,
                    command TEXT NOT NULL,
                    duration INTEGER NOT NULL,
                    created DATETIME NOT NULL,
                    end DATETIME NOT NULL
                );
        ";

        string createBasecommLogTableSql = useMySql ?
            @"
                CREATE TABLE IF NOT EXISTS basecomm_log (
                    player_steamid BIGINT UNSIGNED NOT NULL,
                    player_name varchar(128),
                    admin_steamid BIGINT UNSIGNED,
                    admin_name varchar(128) NOT NULL,
                    command varchar(128) NOT NULL,
                    duration INT,
                    date DATETIME NOT NULL
                );
            "
            :
            @"
                CREATE TABLE IF NOT EXISTS basecomm_log (
                    player_steamid INTEGER NOT NULL,
                    player_name TEXT,
                    admin_steamid INTEGER,
                    admin_name TEXT NOT NULL,
                    command TEXT NOT NULL,
                    duration INTEGER,
                    date DATETIME NOT NULL
                );
        ";

        await connection.ExecuteAsync(createBasebanTableSql, transaction: transaction);
        await connection.ExecuteAsync(createBasebanLogTableSql, transaction: transaction);
        await connection.ExecuteAsync(createBasecommTableSql, transaction: transaction);
        await connection.ExecuteAsync(createBasecommLogTableSql, transaction: transaction);
    }

    public static async Task<bool> IsBannedAsync(ulong steamid)
    {
        using DbConnection connection = await ConnectAsync();
        int? result = await connection.ExecuteScalarAsync<int?>("SELECT 1 FROM baseban WHERE steamid = @steamid LIMIT 1;", new { steamid });

        return result.HasValue;
    }

    public static async Task Ban(CCSPlayerController player, string playername, CCSPlayerController? admin, string adminname, string reason, int duration)
    {
        await Addban(player, playername, player.SteamID, admin, adminname, reason, duration);
    }

    public static async Task Ban(ulong steamid, CCSPlayerController? admin, string adminname, string reason, int duration)
    {
        await Addban(null, Instance.Localizer["Console"], steamid, admin, adminname, reason, duration);
    }

    private static async Task Addban(CCSPlayerController? player, string playername, ulong steamid, CCSPlayerController? admin, string adminname, string reason, int duration)
    {
        DateTime created = DateTime.Now;
        DateTime end = created.AddMinutes(duration);

        using DbConnection connection = await ConnectAsync();
        using DbTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            string insertBasebanSql = Instance.Config.Database.UseMySql ?
            @"
                INSERT INTO baseban (steamid, duration, created, end) 
                VALUES (@steamid, @duration, @created, @end) 
                ON DUPLICATE KEY UPDATE 
                steamid = VALUES(steamid), 
                duration = VALUES(duration), 
                end = VALUES(end), 
                created = VALUES(created);
            "
            :
            @"
                INSERT INTO baseban (steamid, duration, created, end) 
                VALUES (@steamid, @duration, @created, @end);
            ";

            await connection.ExecuteAsync(insertBasebanSql,
            new
            {
                steamid,
                duration,
                created,
                end
            }, transaction: transaction);

            await connection.ExecuteAsync(@"
                INSERT INTO baseban_log (player_steamid, player_name, admin_steamid, admin_name, command, reason, duration, date)
                VALUES (@player_steamid, @player_name, @admin_steamid, @admin_name, 'ban', @reason, @duration, @date);
            ", new
            {
                player_steamid = player?.SteamID ?? steamid,
                @player_name = playername,
                @admin_steamid = admin?.SteamID ?? 0,
                @admin_name = adminname,
                reason,
                duration,
                @date = created
            }, transaction: transaction);

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public static async Task Unban(ulong steamid, CCSPlayerController? admin, string adminname)
    {
        DateTime created = DateTime.Now;

        using DbConnection connection = await ConnectAsync();
        using DbTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            await connection.ExecuteAsync("DELETE FROM baseban WHERE steamid = @steamid;", new { steamid }, transaction: transaction);

            await connection.ExecuteAsync(@"
                INSERT INTO baseban_log (player_steamid, admin_steamid, admin_name, command, date)
                VALUES (@player_steamid, @admin_steamid, @admin_name, 'unban', @date);
            ", new
            {
                @player_steamid = steamid,
                @admin_steamid = admin?.SteamID ?? 0,
                @admin_name = adminname,
                @date = created
            }, transaction: transaction);

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public static async Task LoadPlayer(ulong steamid)
    {
        using DbConnection connection = await ConnectAsync();

        IEnumerable<dynamic> results = await connection.QueryAsync("SELECT * FROM basecomm WHERE steamid = @steamid", new { steamid });

        foreach (dynamic result in results)
        {
            PlayerTemporaryPunishList.Add(new PunishInfo
            {
                SteamID = result.steamid,
                PunishName = result.command,
                Duration = result.duration,
                Created = Convert.ToDateTime(result.created),
                End = Convert.ToDateTime(result.end)
            });
        }
    }

    public static async Task PunishPlayer(CCSPlayerController player, string playername, CCSPlayerController? admin, string adminname, string punishname, int duration)
    {
        DateTime created = DateTime.Now;
        DateTime end = created.AddMinutes(duration);

        using DbConnection connection = await ConnectAsync();
        using DbTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            string insertPunishSql = Instance.Config.Database.UseMySql ?
            @"
                INSERT INTO basecomm (steamid, command, duration, created, end) 
                VALUES (@steamid, @command, @duration, @created, @end) 
                ON DUPLICATE KEY UPDATE 
                steamid = VALUES(steamid), 
                duration = VALUES(duration), 
                end = VALUES(end),
                created = VALUES(created);
            "
            :
            @"
                INSERT OR REPLACE INTO basecomm (steamid, command, duration, created, end) 
                VALUES (@steamid, @command, @duration, @created, @end);
            ";

            await connection.ExecuteAsync(insertPunishSql, new
            {
                steamid = player.SteamID,
                command = punishname,
                duration,
                created,
                end
            }, transaction: transaction);

            await connection.ExecuteAsync(@"
                INSERT INTO basecomm_log (player_steamid, player_name, admin_steamid, admin_name, command, duration, date)
                VALUES (@player_steamid, @player_name, @admin_steamid, @admin_name, @command, @duration, @date);
            ", new
            {
                player_steamid = player.SteamID,
                player_name = playername,
                admin_steamid = admin?.SteamID ?? 0,
                admin_name = adminname,
                command = punishname,
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

    public static async Task UnPunishPlayer(ulong steamid, CCSPlayerController? admin, string adminname, string punishname)
    {
        DateTime created = DateTime.Now;

        using DbConnection connection = await ConnectAsync();
        using DbTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            await connection.ExecuteAsync("DELETE FROM basecomm WHERE steamid = @steamid AND command = @command;", new { steamid, @command = punishname }, transaction: transaction);

            await connection.ExecuteAsync(@"
                INSERT INTO baseban_log (player_steamid, admin_steamid, admin_name, command, date)
                VALUES (@player_steamid, @admin_steamid, @admin_name, @command, @date);
            ", new
            {
                @player_steamid = steamid,
                @admin_steamid = admin?.SteamID ?? 0,
                @admin_name = adminname,
                @command = punishname,
                @date = created
            }, transaction: transaction);

            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public static async Task RemoveExpiredPunishs()
    {
        using DbConnection connection = await ConnectAsync();

        await connection.ExecuteAsync("DELETE FROM basecomm WHERE end < @end", new { @end = DateTime.Now });
    }

    public static async Task RemoveExpiredBans()
    {
        using DbConnection connection = await ConnectAsync();

        await connection.ExecuteAsync("DELETE FROM baseban WHERE end < @end", new { @end = DateTime.Now });
    }
}
