using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using Dapper;
using MySqlConnector;
using static Admin.Admin;

namespace Admin;

public static class Database
{
    private static string? GlobalDatabaseConnectionString;
    private static bool IsDatabaseEnabled => GlobalDatabaseConnectionString is not null;

    private static async Task<MySqlConnection> ConnectAsync()
    {
        MySqlConnection connection = new(GlobalDatabaseConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    private static void ExecuteAsync(string query, object? parameters)
    {
        if (!IsDatabaseEnabled)
        {
            return;
        }
        Task.Run(async () =>
        {
            using MySqlConnection connection = await ConnectAsync();
            await connection.ExecuteAsync(query, parameters);
        });
    }
    public static async Task CreateDatabaseAsync(AdminConfig config)
    {
        if (!config.DatabaseEnabled)
        {
            return;
        }

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
                        created DATETIME NOT NULL,
                        end DATETIME NOT NULL,
                        PRIMARY KEY (id),
                        UNIQUE KEY id (id),
                        UNIQUE KEY steamid (steamid)
            );
            ", transaction: transaction);

            await connection.ExecuteAsync(@"
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
            ", transaction: transaction);

            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS basecomm (
                        id INT NOT NULL AUTO_INCREMENT,
                        steamid BIGINT UNSIGNED NOT NULL,
                        command varchar(128) NOT NULL,
                        duration INT NOT NULL,
                        created DATETIME NOT NULL,
                        end DATETIME NOT NULL,
                        PRIMARY KEY (id),
                        UNIQUE KEY id (id)
            );
            ", transaction: transaction);

            await connection.ExecuteAsync(@"
                CREATE TABLE IF NOT EXISTS basecomm_log (
                        player_steamid BIGINT UNSIGNED NOT NULL,
                        player_name varchar(128),
                        admin_steamid BIGINT UNSIGNED,
                        admin_name varchar(128) NOT NULL,
                        command varchar(128) NOT NULL,
                        duration INT,
                        date DATETIME NOT NULL
                );
            ", transaction: transaction);

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
        if (!IsDatabaseEnabled)
        {
            return false;
        }

        using MySqlConnection connection = await ConnectAsync();

        IEnumerable<dynamic> results = await connection.QueryAsync("SELECT * FROM baseban WHERE steamid = @steamid;", new { steamid });

        return results.Any();
    }

    public static async Task Ban(CCSPlayerController player, string playername, CCSPlayerController? admin, string adminname, string reason, int duration)
    {
        await Addban(player, playername, player.SteamID, admin, adminname, reason, duration);
    }

    public static async Task Ban(ulong steamid, CCSPlayerController? admin, string adminname, string reason, int duration)
    {
        await Addban(null, "Console", steamid, admin, adminname, reason, duration);
    }

    private static async Task Addban(CCSPlayerController? player, string playername, ulong steamid, CCSPlayerController? admin, string adminname, string reason, int duration)
    {
        DateTime created = DateTime.Now;
        DateTime end = created.AddMinutes(duration);

        using MySqlConnection connection = await ConnectAsync();
        using MySqlTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            await connection.ExecuteAsync(@"
            INSERT INTO baseban (steamid, duration, created, end) 
            VALUES (@steamid, @duration, @created, @end) 
            ON DUPLICATE KEY UPDATE 
            steamid = VALUES(steamid), 
            duration = VALUES(duration), 
            end = VALUES(end), 
            created = VALUES(created);
        ", new
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

        using MySqlConnection connection = await ConnectAsync();
        using MySqlTransaction transaction = await connection.BeginTransactionAsync();

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

    public static async Task LoadPlayer(CCSPlayerController player)
    {
        if (!IsDatabaseEnabled)
        {
            return;
        }

        using MySqlConnection connection = await ConnectAsync();

        IEnumerable<dynamic> results = await connection.QueryAsync("SELECT * FROM basecomm WHERE steamid = @steamid", new { steamid = player.SteamID });

        foreach (dynamic result in results)
        {
            if (result.command == "GAG")
            {
                TagApi?.GagPlayer(player.SteamID);
            }

            PlayerTemporaryPunishList.Add(new PunishInfo
            {
                SteamID = result.steamid,
                PunishName = result.command,
                Duration = result.duration,
                Created = ((MySqlDateTime)result.created).GetDateTime(),
                End = ((MySqlDateTime)result.end).GetDateTime()
            });
        }
    }

    public static async Task PunishPlayer(CCSPlayerController player, string playername, CCSPlayerController? admin, string adminname, string punishname, int duration)
    {
        DateTime created = DateTime.Now;
        DateTime end = created.AddMinutes(duration);

        using MySqlConnection connection = await ConnectAsync();
        using MySqlTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            await connection.ExecuteAsync(@"
            INSERT INTO basecomm (steamid, command, duration, created, end) 
            VALUES (@steamid, @command, @duration, @created, @end) 
            ON DUPLICATE KEY UPDATE 
            steamid = VALUES(steamid), 
            duration = VALUES(duration), 
            end = VALUES(end),
            created = VALUES(created);
        ", new
            {
                @steamid = player.SteamID,
                @command = punishname,
                duration,
                created,
                end
            }, transaction: transaction);

            await connection.ExecuteAsync(@"
            INSERT INTO basecomm_log (player_steamid, player_name, admin_steamid, admin_name, command, duration, date)
            VALUES (@player_steamid, @player_name, @admin_steamid, @admin_name, @command, @duration, @date);
        ", new
            {
                @player_steamid = player.SteamID,
                @player_name = playername,
                @admin_steamid = admin?.SteamID ?? 0,
                @admin_name = adminname,
                @command = punishname,
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

    public static async Task UnPunishPlayer(ulong steamid, CCSPlayerController? admin, string adminname, string punishname)
    {
        DateTime created = DateTime.Now;

        using MySqlConnection connection = await ConnectAsync();
        using MySqlTransaction transaction = await connection.BeginTransactionAsync();

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
        if (!IsDatabaseEnabled)
        {
            return;
        }

        using MySqlConnection connection = await ConnectAsync();

        await connection.ExecuteAsync("DELETE FROM basecomm WHERE end < @end", new { @end = DateTime.Now });
    }

    public static async Task RemoveExpiredBans()
    {
        if (!IsDatabaseEnabled)
        {
            return;
        }

        using MySqlConnection connection = await ConnectAsync();

        await connection.ExecuteAsync("DELETE FROM baseban WHERE end < @end", new { @end = DateTime.Now });
    }
}
