using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using System.Numerics;

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

        if(savedatabase)
        {
            SaveDatabase(p, punishmentname == "ban" ? "baseban" : "basecomm");
        } 
    }
    public void AddPunishmentForPlayer(CCSPlayerController? player, ulong steamid, string punishmentname, string reason, int duration, bool savedatabase)
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
    }
    public void RemovePunishment(ulong steamid, string punishmentname, bool savedatabase)
    {
        GlobalPunishList.RemoveAll(p => p.PlayerSteamid == steamid && p.PunishmentName == punishmentname && p.SaveDatabase == savedatabase);

        if(savedatabase)
        {
            RemoveFromDatabase(steamid, punishmentname == "ban" ? "baseban" : "basecomm");
        }
    }
    public void RemoveExpiredPunishments()
    {
        if(GlobalPunishList.RemoveAll(p => p.End < DateTime.Now && p.Duration != -1 && p.SaveDatabase == true) > 0)
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

    public void RemoveWeaponsOnTheGround()
    {
        foreach (string weapon in GlobalWeaponGroundList)
        {
            var entities = Utilities.FindAllEntitiesByDesignerName<CCSWeaponBaseGun>("weapon_" + weapon);

            foreach (var entity in entities)
            {
                if (entity.State == CSWeaponState_t.WEAPON_NOT_CARRIED)
                {
                    entity.Remove();
                }
            }
        }
    }

    public string GetPlayerNameOrConsole(CCSPlayerController? player)
    {
        return player == null ? Localizer["Console"] : player.PlayerName;
    }

    public static string GetPlayerSteamIdOrConsole(CCSPlayerController? player)
    {
        return player == null ? "[CONSOLE]" : player.SteamID.ToString();
    }
}