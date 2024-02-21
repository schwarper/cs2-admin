using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using System.Numerics;
using System.Text;

namespace Admin;

public partial class Admin : BasePlugin
{
    public void SetPunishmentForPlayer(CCSPlayerController? player, CCSPlayerController target, string punishmentname, string reason, int duration, bool savedatabase)
    {
        if (duration <= 0)
        {
            duration = -1;
        }

        DateTime now = DateTime.Now;

        Punishment p = new()
        {
            PlayerSteamid = target.SteamID,
            PlayerName = target.PlayerName,
            AdminSteamid = player == null ? 0 : player.SteamID,
            AdminName = GetPlayerNameOrConsole(player),
            PunishmentName = punishmentname,
            Reason = reason,
            Duration = duration,
            End = (duration == -1) ? DateTime.MinValue : now.AddMinutes(duration),
            Created = now,
            SaveDatabase = savedatabase
        };

        GlobalPunishList.Add(p);

        if (savedatabase)
        {
            SaveDatabase(p, punishmentname == "ban" ? "baseban" : "basecomm");
        }

        if (punishmentname == "gag")
        {
            Server.ExecuteCommand($"css_tag_mute {target.SteamID}");
        }
    }
    public void AddPunishmentForPlayer(CCSPlayerController? player, ulong steamid, string reason, int duration, bool savedatabase)
    {
        if (duration <= 0)
        {
            duration = -1;
        }

        DateTime now = DateTime.Now;

        Punishment p = new()
        {
            PlayerSteamid = steamid,
            PlayerName = "null",
            AdminSteamid = player == null ? 0 : player.SteamID,
            AdminName = GetPlayerNameOrConsole(player),
            PunishmentName = "baseban",
            Reason = reason,
            Duration = duration,
            End = (duration == -1) ? DateTime.MinValue : now.AddMinutes(duration),
            Created = now,
            SaveDatabase = savedatabase
        };

        GlobalPunishList.Add(p);

        if (savedatabase)
        {
            SaveDatabase(p, "baseban");
        }
    }
    public void RemovePunishment(ulong steamid, string punishmentname, bool savedatabase)
    {
        GlobalPunishList.RemoveAll(p => p.PlayerSteamid == steamid && p.PunishmentName == punishmentname && p.SaveDatabase == savedatabase);

        if (savedatabase)
        {
            RemoveFromDatabase(steamid, punishmentname == "ban" ? "baseban" : "basecomm");
        }

        if (punishmentname == "gag")
        {
            Punishment? tgag = GlobalPunishList.FirstOrDefault(p => p.PlayerSteamid == steamid && p.PunishmentName == "gag" && p.SaveDatabase);

            if (tgag == null || !savedatabase)
            {
                Server.ExecuteCommand($"css_tag_unmute {steamid}");
            }
        }
    }
    public void RemoveExpiredPunishments()
    {
        bool remove = false;

        List<Punishment> punishmentsToRemove = GlobalPunishList
            .Where(p => p.End < DateTime.Now && p.Duration != -1 && p.SaveDatabase)
            .ToList();

        foreach (Punishment? punishment in punishmentsToRemove)
        {
            remove = true;

            if (punishment.PunishmentName == "gag")
            {
                Server.ExecuteCommand($"css_tag_unmute {punishment.PlayerSteamid}");
            }

            GlobalPunishList.Remove(punishment);
        }

        if (remove)
        {
            RemoveExpiredFromDatabase();
        }
    }
    public bool IsPlayerPunished(CCSPlayerController player, string punishmentname)
    {
        return GlobalPunishList.Exists(p => p.PlayerSteamid == player.SteamID && p.PunishmentName == punishmentname);
    }
    public void KickPlayer(CCSPlayerController player, string reason)
    {
        AddTimer(Config.KickDelay, () =>
        {
            player.Kick(reason);
        });
    }
    public static string GetCvarStringValue(ConVar cvar)
    {
        ConVarType cvartype = cvar.Type;

        return cvartype switch
        {
            ConVarType.Bool => cvar.GetPrimitiveValue<bool>().ToString(),
            ConVarType.Int16 => cvar.GetPrimitiveValue<Int16>().ToString(),
            ConVarType.UInt16 => cvar.GetPrimitiveValue<UInt16>().ToString(),
            ConVarType.Int32 => cvar.GetPrimitiveValue<int>().ToString(),
            ConVarType.UInt32 => cvar.GetPrimitiveValue<UInt32>().ToString(),
            ConVarType.Int64 => cvar.GetPrimitiveValue<Int64>().ToString(),
            ConVarType.UInt64 => cvar.GetPrimitiveValue<UInt64>().ToString(),
            ConVarType.Float32 => cvar.GetPrimitiveValue<float>().ToString(),
            ConVarType.Float64 => cvar.GetPrimitiveValue<double>().ToString(),
            ConVarType.String => cvar.StringValue,
            ConVarType.Color => cvar.GetPrimitiveValue<Color>().ToString(),
            ConVarType.Vector2 => cvar.GetPrimitiveValue<Vector2>().ToString(),
            ConVarType.Vector3 => cvar.GetPrimitiveValue<Vector3>().ToString(),
            ConVarType.Vector4 => cvar.GetPrimitiveValue<Vector4>().ToString(),
            ConVarType.Qangle => cvar.GetPrimitiveValue<QAngle>().ToString(),
            ConVarType.Invalid => "Invalid",
            _ => "Invalid"
        };
    }

    public static void RemoveWeaponsOnTheGround()
    {
        IEnumerable<CCSWeaponBaseGun> entities = Utilities.FindAllEntitiesByDesignerName<CCSWeaponBaseGun>("weapon_")
            .Where(entity => entity is { IsValid: true, State: CSWeaponState_t.WEAPON_NOT_CARRIED } && entity.DesignerName.StartsWith("weapon_"));

        foreach(CCSWeaponBaseGun entity in entities)
        {
            entity.Remove();
        }
    }

    public static string GetPlayerNameOrConsole(CCSPlayerController? player)
    {
        return player == null ? "Console" : player.PlayerName;
    }

    public static string GetPlayerSteamIdOrConsole(CCSPlayerController? player)
    {
        return player == null ? "[CONSOLE]" : player.SteamID.ToString();
    }

    public void PrintToChatAll(string message, params object[] args)
    {
        foreach (CCSPlayerController player in Utilities.GetPlayers().Where(p => p != null && p.Valid()))
        {
            object[] modifiedArgs = args;

            if (args.Length > 0 && args[0]?.ToString() == "Console")
            {
                if (Config.HideConsoleMsg)
                {
                    continue;
                }

                modifiedArgs = new string[args.Length];
                Array.Copy(args, modifiedArgs, args.Length);
                modifiedArgs[0] = Localizer["Console"];
            }

            if (Config.ShowNameCommands.Contains(message.Split('<').First()) && Config.ShowNameFlag != string.Empty && !AdminManager.PlayerHasPermissions(player, Config.ShowNameFlag))
            {
                modifiedArgs[0] = Localizer["Console"];
            }

            using (new WithTemporaryCulture(player.GetLanguage()))
            {
                StringBuilder builder = new(Localizer["Prefix"]);
                builder.AppendFormat(Localizer[message], modifiedArgs);
                player.PrintToChat(builder.ToString());
            }
        }
    }
}