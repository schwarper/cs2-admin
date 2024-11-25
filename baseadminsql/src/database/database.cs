using CounterStrikeSharp.API.Modules.Admin;
using Dapper;
using MySqlConnector;
using System.Text.Json;

namespace BaseAdminSql;

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
        if (string.IsNullOrEmpty(config.Database.Host) ||
            string.IsNullOrEmpty(config.Database.Name) ||
            string.IsNullOrEmpty(config.Database.User))
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

        await using MySqlConnection connection = await ConnectAsync();
        await using MySqlTransaction transaction = await connection.BeginTransactionAsync();

        try
        {
            await connection.ExecuteAsync(CreateBaseadminsqlAdminsTable, transaction: transaction);
            await connection.ExecuteAsync(CreateBaseadminsqlGroupsTable, transaction: transaction);
            await transaction.CommitAsync();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public static async Task LoadAdmins()
    {
        await using MySqlConnection connection = await ConnectAsync();

        IEnumerable<dynamic> admins = await connection.QueryAsync(SelectBaseadminsqlAdminsSql);

        foreach (dynamic admin in admins)
        {
            HashSet<string> flags = JsonSerializer.Deserialize<HashSet<string>>(admin.flags.ToString()) ?? new HashSet<string>();
            HashSet<string> groups = JsonSerializer.Deserialize<HashSet<string>>(admin.groups.ToString()) ?? new HashSet<string>();

            AdminManagerEx.AddAdmin(admin.steamid, flags, groups, (uint)admin.immunity);
        }

        DateTime now = DateTime.Now;
        string formattedTime = now.ToString("HH:mm:ss");
        Console.WriteLine($"{formattedTime} [INFO](Base Admin SQL) Loaded admin data with {admins.Count()} admins.");

        AdminManager.MergeGroupPermsIntoAdmins();
    }

    public static async Task LoadGroups()
    {
        await using MySqlConnection connection = await ConnectAsync();

        IEnumerable<dynamic> groups = await connection.QueryAsync(SelectBaseadminsqlGroupsSql);

        foreach (dynamic group in groups)
        {
            HashSet<string> flags = JsonSerializer.Deserialize<HashSet<string>>(group.flags.ToString()) ?? new HashSet<string>();

            AdminManagerEx.AddGroup(group.group, flags, (uint)group.immunity);
        }

        DateTime now = DateTime.Now;
        string formattedTime = now.ToString("HH:mm:ss");
        Console.WriteLine($"{formattedTime} [INFO](Base Admin SQL) Loaded group data with {groups.Count()} groups.");
    }

    public static async Task AddAdmin(ulong steamid, HashSet<string> flags, HashSet<string> groups, uint immunity)
    {
        await using MySqlConnection connection = await ConnectAsync();

        await connection.ExecuteAsync(InsertBaseadminsqlAdminsSql, new
        {
            steamid,
            flags = JsonSerializer.Serialize(flags),
            groups = JsonSerializer.Serialize(groups),
            immunity
        });

        AdminManagerEx.AddAdmin(steamid, flags, groups, immunity);
        AdminManager.MergeGroupPermsIntoAdmins();
    }

    public static async Task RemoveAdmin(ulong steamid)
    {
        await using MySqlConnection connection = await ConnectAsync();

        await connection.ExecuteAsync(DeleteBasadminsqlAdminsSql, new
        {
            steamid
        });

        AdminManagerEx.RemoveAdmin(steamid);
    }

    public static async Task AddGroup(string group, HashSet<string> flags, uint immunity)
    {
        await using MySqlConnection connection = await ConnectAsync();

        await connection.ExecuteAsync(InsertBaseadminsqlGroupsSql, new
        {
            group,
            flags = JsonSerializer.Serialize(flags),
            immunity
        });

        AdminManagerEx.AddGroup(group, flags, immunity);
    }

    public static async Task RemoveGroup(string group)
    {
        await using MySqlConnection connection = await ConnectAsync();

        await connection.ExecuteAsync(DeleteBasadminsqlGroupsSql, new
        {
            group
        });

        AdminManagerEx.RemoveGroup(group);
    }

    private const string CreateBaseadminsqlAdminsTable = @"
        CREATE TABLE IF NOT EXISTS baseadminsql_admins (
            id INT AUTO_INCREMENT PRIMARY KEY,
            steamid BIGINT UNSIGNED NOT NULL UNIQUE,
            flags JSON NOT NULL,
            `groups` JSON NOT NULL,
            immunity INT DEFAULT 0
        );
    ";

    private const string CreateBaseadminsqlGroupsTable = @"
        CREATE TABLE IF NOT EXISTS baseadminsql_groups (
            id INT AUTO_INCREMENT PRIMARY KEY,
            `group` VARCHAR(255) NOT NULL UNIQUE,
            flags JSON NOT NULL,
            immunity INT DEFAULT 0
        );
    ";

    private const string InsertBaseadminsqlAdminsSql = @"
        INSERT INTO baseadminsql_admins (steamid, flags, groups, immunity)
        VALUES (@steamid, @flags, @groups, @immunity);
    ";

    private const string InsertBaseadminsqlGroupsSql = @"
        INSERT INTO baseadminsql_groups (`group`, flags, immunity)
        VALUES (@group, @flags, @immunity);
    ";

    private const string SelectBaseadminsqlAdminsSql = @"
        SELECT * FROM baseadminsql_admins;
    ";

    private const string SelectBaseadminsqlGroupsSql = @"
        SELECT * FROM baseadminsql_groups;
    ";

    private const string DeleteBasadminsqlAdminsSql = @"
        DELETE FROM baseadminsql_admins WHERE steamid = @steamid;
    ";

    private const string DeleteBasadminsqlGroupsSql = @"
        DELETE FROM baseadminsql_groups WHERE `group` = @group;
    ";
}