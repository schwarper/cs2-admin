using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using Dapper;
using MySqlConnector;

namespace Admin;

public partial class Admin : BasePlugin
{
    private void CreateDatabase(AdminConfig config)
    {
        if (config.DatabaseHost.Length < 1 || config.DatabaseName.Length < 1 || config.DatabaseUser.Length < 1)
        {
            throw new Exception("[cs2-admin] You need to setup Database credentials in config!");
        }

        MySqlConnectionStringBuilder builder = new()
        {
            Server = config.DatabaseHost,
            Database = config.DatabaseName,
            UserID = config.DatabaseUser,
            Password = config.DatabasePassword,
            Port = (uint)config.DatabasePort,
            Pooling = true,
        };

        string connectionstring = builder.ConnectionString;

        try
        {
            GlobalDatabase = new(connectionstring);
            GlobalDatabase.Open();
        }
        catch (Exception)
        {
            throw new Exception("Cannot connect database");
        }

        Task.Run(async () =>
        {
            using MySqlTransaction transaction = await GlobalDatabase.BeginTransactionAsync();

            try
            {
                await GlobalDatabase.QueryAsync(@"
                    CREATE TABLE IF NOT EXISTS basecomm (
                        id INT NOT NULL AUTO_INCREMENT,
                        player_steamid BIGINT UNSIGNED NOT NULL,
                        player_name varchar(128) NOT NULL,
                        admin_steamid BIGINT UNSIGNED NOT NULL,
                        admin_name varchar(128) NOT NULL,
                        punishment varchar(32) NOT NULL,
                        reason varchar(32) NOT NULL,
                        duration INT NOT NULL,
                        end TIMESTAMP NOT NULL,
                        created TIMESTAMP NOT NULL,
                        PRIMARY KEY (id)
                );", transaction: transaction);

                await GlobalDatabase.ExecuteAsync(@"
                    CREATE TABLE IF NOT EXISTS baseban (
                        id INT NOT NULL AUTO_INCREMENT,
                        player_steamid BIGINT UNSIGNED NOT NULL,
                        player_name varchar(128) NOT NULL,
                        admin_steamid BIGINT UNSIGNED NOT NULL,
                        admin_name varchar(128) NOT NULL,
                        punishment varchar(32) NOT NULL,
                        reason varchar(32) NOT NULL,
                        duration INT NOT NULL,
                        end TIMESTAMP NOT NULL,
                        created TIMESTAMP NOT NULL,
                        PRIMARY KEY (id)
                );", transaction: transaction);

                await transaction.CommitAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        });
    }
    private void LoadDatabase()
    {
        Task.Run(async () =>
        {
            dynamic? result = await GlobalDatabase.QueryFirstOrDefaultAsync(@"SELECT * FROM basecomm UNION SELECT * FROM baseban;");

            Server.NextFrame(() =>
            {
                if (result == null)
                {
                    return;
                }

                GlobalPunishList.Add(new Punishment
                {
                    PlayerSteamid = result.player_steamid,
                    PlayerName = result.player_name,
                    AdminSteamid = result.admin_steamid,
                    AdminName = result.admin_name,
                    PunishmentName = result.punishment,
                    Reason = result.reason,
                    Duration = result.duration,
                    End = result.end,
                    Created = result.created,
                    SaveDatabase = true
                });
            });
        });
    }

    private void SaveDatabase(Punishment @punishment, string table)
    {
        DateTime now = DateTime.Now;

        Task.Run(async () =>
        {
            await GlobalDatabase.ExecuteAsync(@$"
                INSERT INTO {table} (
                    player_steamid, player_name, admin_steamid, admin_name, punishment, reason, duration, end, created
                ) VALUES (
                    @player_steamid, @player_name, @admin_steamid, @admin_name, @punishment, @reason, @duration, @end, @created
                ) ON DUPLICATE KEY UPDATE
                    player_name = VALUES(player_name),
                    admin_steamid = VALUES(admin_steamid),
                    admin_name = VALUES(admin_name),
                    punishment = VALUES(punishment),
                    reason = VALUES(reason),
                    duration = VALUES(duration),
                    end = VALUES(end),
                    created = VALUES(created);
            ",
            new
            {
                player_steamid = @punishment.PlayerSteamid,
                player_name = @punishment.PlayerName,
                admin_steamid = @punishment.AdminSteamid,
                admin_name = @punishment.AdminName,
                punishment = @punishment.PunishmentName,
                reason = @punishment.Reason,
                duration = @punishment.Duration,
                end = now.AddMinutes(@punishment.Duration),
                created = now
            });
        });
    }

    private void RemoveFromDatabase(ulong steamid, string table)
    {
        Task.Run(async () =>
        {
            await GlobalDatabase.ExecuteAsync(@$"
                DELETE FROM {table} WHERE player_steamid = @player_steamid;
            ",
            new
            {
                player_steamid = steamid
            });
        });
    }

    private void RemoveExpiredFromDatabase()
    {
        Task.Run(() =>
        {
            GlobalDatabase.Query(@"DELETE FROM basecomm WHERE end < CURRENT_TIMESTAMP;");
            GlobalDatabase.Query(@"DELETE FROM baseban WHERE end < CURRENT_TIMESTAMP;");
        });
    }
}