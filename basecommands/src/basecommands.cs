using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using System.Numerics;

namespace BaseCommands;

public class BaseCommands : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Basic Commands";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "schwarper";

    public Config Config { get; set; } = new Config();

    public void OnConfigParsed(Config config)
    {
        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);
        Config = config;
    }

    [ConsoleCommand("css_kick")]
    [RequiresPermissions("@css/kick")]
    [CommandHelper(minArgs: 1, usage: "<#userid|name> [reason]")]
    public void Command_Kick(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries); ;

        TargetResult targetResult = new Target(args[0]).GetTarget(player);

        if (targetResult.Players.Count == 0)
        {
            info.ReplyToCommand(Config.Tag + Localizer["No matching client"]);
            return;
        }
        else if (targetResult.Players.Count > 1)
        {
            info.ReplyToCommand(Config.Tag + Localizer["More than one client matched"]);
            return;
        }

        CCSPlayerController target = targetResult.Players[0];

        if (!AdminManager.CanPlayerTarget(player, target))
        {
            info.ReplyToCommand(Config.Tag + Localizer["Unable to target"]);
            return;
        }

        string adminname = player?.PlayerName ?? Localizer["Console"];
        string targetname = target.PlayerName;
        string reason = string.Join(' ', args[1..]);

        if (string.IsNullOrWhiteSpace(reason))
        {
            SendMessageToAllPlayers(HudDestination.Chat, "Kicked reason", adminname, targetname, reason);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "Kicked", adminname, targetname);
        }
    }

    [ConsoleCommand("css_changemap")]
    [ConsoleCommand("css_map")]
    [RequiresPermissions("@css/changemap")]
    [CommandHelper(minArgs: 1, usage: "<map>")]
    public void Command_Map(CCSPlayerController? player, CommandInfo info)
    {
        string arg = info.GetArg(0);

        if (!Server.IsMapValid(arg))
        {
            if (Config.WorkshopMapName.TryGetValue(arg, out ulong workshopMapId))
            {
                ExecuteMapCommand(player, arg, $"host_workshop_map {workshopMapId}", true);
                return;
            }

            info.ReplyToCommand(Config.Tag + Localizer["Map was not found", arg]);
            return;
        }

        ExecuteMapCommand(player, arg, $"changelevel {arg}", false);
    }

    [ConsoleCommand("css_wsmap")]
    [ConsoleCommand("css_workshop")]
    [RequiresPermissions("@css/changemap")]
    [CommandHelper(minArgs: 1, usage: "<map>")]
    public void Command_WsMap(CCSPlayerController? player, CommandInfo info)
    {
        string arg = info.GetArg(0);
        string mapCommand;

        if (!ulong.TryParse(arg, out ulong workshopMapId))
        {
            mapCommand = $"ds_workshop_changelevel {arg}";
        }
        else
        {
            string workshopName = Config.WorkshopMapName.FirstOrDefault(p => p.Value == workshopMapId).Key;

            if (workshopName == null)
            {
                mapCommand = $"host_workshop_map {arg}";
            }
            else
            {
                mapCommand = $"host_workshop_map {workshopMapId}";
                arg = workshopName;
            }
        }

        ExecuteMapCommand(player, arg, mapCommand, true);
    }

    [ConsoleCommand("css_rcon")]
    [RequiresPermissions("@css/rcon")]
    [CommandHelper(minArgs: 1, usage: "<args>")]
    public void Command_Rcon(CCSPlayerController? player, CommandInfo info)
    {
        string arg = info.ArgString;

        Server.ExecuteCommand(arg);

        string adminname = player?.PlayerName ?? Localizer["Console"];

        SendMessageToAllPlayers(HudDestination.Chat, "Sent Rcon", adminname, arg);
    }

    [ConsoleCommand("css_cvar")]
    [RequiresPermissions("@css/cvar")]
    [CommandHelper(minArgs: 1, usage: "<cvar> <value>")]
    public void Command_Cvar(CCSPlayerController? player, CommandInfo info)
    {
        string cvarname = info.GetArg(1);

        ConVar? cvar = ConVar.Find(cvarname);

        if (cvar == null)
        {
            info.ReplyToCommand(Config.Tag + Localizer["Cvar is not found", cvarname]);
            return;
        }

        if (cvar.Name.Equals("sv_cheats") && !AdminManager.PlayerHasPermissions(player, "@css/cheats"))
        {
            info.ReplyToCommand(Config.Tag + Localizer["You don't have permissions to change sv_cheats"]);
            return;
        }

        string value;

        if (info.ArgCount < 3)
        {
            value = GetCvarStringValue(cvar);

            info.ReplyToCommand(Config.Tag + Localizer["Cvar value", cvar.Name, value]);
            return;
        }

        value = info.GetArg(2);

        Server.ExecuteCommand($"{cvarname} {value}");

        string adminname = player?.PlayerName ?? Localizer["Console"];

        SendMessageToAllPlayers(HudDestination.Chat, "Cvar changed", adminname, cvar.Name, value);
    }

    [ConsoleCommand("css_exec")]
    [RequiresPermissions("@css/config")]
    [CommandHelper(minArgs: 1, usage: "<exec>")]
    public void Command_Exec(CCSPlayerController? player, CommandInfo info)
    {
        string cfg = info.ArgString;

        Server.ExecuteCommand($"exec {cfg}");

        string adminname = player?.PlayerName ?? Localizer["Console"];

        SendMessageToAllPlayers(HudDestination.Chat, "Executed config", adminname, cfg);
    }

    [ConsoleCommand("css_who")]
    [RequiresPermissions("@css/generic")]
    [CommandHelper(minArgs: 0, usage: "<#userid|name or empty for all>")]
    public void Command_Who(CCSPlayerController? player, CommandInfo info)
    {
        Action<string> targetConsolePrint = (player != null) ? player.PrintToConsole : Server.PrintToConsole;

        if (info.ArgCount == 1)
        {
            TargetResult targetResult = info.GetArgTargetResult(1);

            if (targetResult.Players.Count == 0)
            {
                info.ReplyToCommand(Config.Tag + Localizer["No matching client"]);
                return;
            }
            else if (targetResult.Players.Count > 1)
            {
                info.ReplyToCommand(Config.Tag + Localizer["More than one client matched"]);
                return;
            }

            CCSPlayerController target = targetResult.Players[0];

            if (!AdminManager.CanPlayerTarget(player, target))
            {
                info.ReplyToCommand(Config.Tag + Localizer["Unable to target"]);
                return;
            }

            PrintPlayerInfo(targetConsolePrint, target, target.PlayerName, true);
            return;
        }

        List<CCSPlayerController> players = Utilities.GetPlayers();

        foreach (CCSPlayerController target in players)
        {
            if (!AdminManager.CanPlayerTarget(player, target))
            {
                continue;
            }

            PrintPlayerInfo(targetConsolePrint, target, string.Empty, false);
        }
    }

    [ConsoleCommand("css_rr")]
    [RequiresPermissions("@css/root")]
    public void Command_RestartRound(CCSPlayerController? player, CommandInfo info)
    {
        Server.ExecuteCommand("mp_restartgame 2");

        string adminname = player?.PlayerName ?? Localizer["Console"];

        SendMessageToAllPlayers(HudDestination.Chat, "Restarted Round", adminname);
    }

    private void ExecuteMapCommand(CCSPlayerController? player, string map, string mapCommand, bool workshop)
    {
        string adminname = player?.PlayerName ?? Localizer["Console"];

        if (Config.ChangeMapDelay > 0)
        {
            AddTimer(Config.ChangeMapDelay, () => Server.ExecuteCommand(mapCommand));
        }
        else
        {
            Server.ExecuteCommand(mapCommand);
        }

        if (workshop)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "Changing map", adminname, map);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "Changing wsmap", adminname, map);
        }
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

    public void PrintPlayerInfo(Action<string> printer, CCSPlayerController player, string targetname, bool singletarget)
    {
        AdminData? data = AdminManager.GetPlayerAdminData(player);

        string permissionflags = (data == null) ? "none" :
            data.GetAllFlags().Contains("@css/root") ? "root" :
            string.Join(",", data.GetAllFlags()).Replace("@css/", "");

        uint immunitylevel = AdminManager.GetPlayerImmunity(player);

        if (singletarget)
        {
            printer(Localizer["css_who<title>", targetname]);
            printer(Localizer["css_who<steamid>", player.SteamID]);
            printer(Localizer["css_who<ip>", player.IpAddress ?? Localizer["Unknown"]]);
            printer(Localizer["css_who<permission>", permissionflags]);
            printer(Localizer["css_who<immunitylevel>", immunitylevel]);
        }
        else
        {
            printer(Localizer["css_who<all>", player.PlayerName, player.SteamID, player.IpAddress ?? Localizer["Unknown"], permissionflags, immunitylevel]);
        }
    }

    private void SendMessageToAllPlayers(HudDestination destination, string messageKey, params object[] args)
    {
        Microsoft.Extensions.Localization.LocalizedString message = Localizer[messageKey, args];
        VirtualFunctions.ClientPrintAll(destination, Config.Tag + message, 0, 0, 0, 0);
    }
}