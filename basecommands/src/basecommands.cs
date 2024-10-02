using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.ValveConstants.Protobuf;
using static BaseCommands.Library;

namespace BaseCommands;

public class BaseCommands : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Basic Commands";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "schwarper";
    public override string ModuleDescription => "Basic Admin Commands";

    public static BaseCommands Instance { get; set; } = new();
    public Config Config { get; set; } = new Config();

    public override void Load(bool hotReload)
    {
        Instance = this;
        LoadValidMaps();
    }

    public void OnConfigParsed(Config config)
    {
        config.Tag = config.Tag.ReplaceColorTags();
        Config = config;
    }

    [ConsoleCommand("css_kick")]
    [RequiresPermissions("@css/kick")]
    [CommandHelper(minArgs: 1, usage: "<#userid|name> [reason]")]
    public void Command_Kick(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        TargetResult targetResult = new Target(args[0]).GetTarget(player);

        if (targetResult.Players.Count == 0)
        {
            SendMessageToReplyToCommand(info, "No matching client");
            return;
        }
        else if (targetResult.Players.Count > 1)
        {
            SendMessageToReplyToCommand(info, "More than one client matched");
            return;
        }

        CCSPlayerController target = targetResult.Players[0];

        if (!AdminManager.CanPlayerTarget(player, target))
        {
            SendMessageToReplyToCommand(info, "Unable to target");
            return;
        }

        string adminname = player?.PlayerName ?? Localizer["Console"];
        string targetname = target.PlayerName;
        string reason = string.Join(' ', args[1..]);

        if (!string.IsNullOrWhiteSpace(reason))
        {
            SendMessageToAllPlayers(HudDestination.Chat, "Kicked reason", adminname, targetname, reason);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "Kicked", adminname, targetname);
        }

        target.Disconnect(NetworkDisconnectionReason.NETWORK_DISCONNECT_KICKED);
    }

    [ConsoleCommand("css_changemap")]
    [ConsoleCommand("css_map")]
    [RequiresPermissions("@css/changemap")]
    [CommandHelper(minArgs: 1, usage: "<map>")]
    public void Command_Map(CCSPlayerController? player, CommandInfo info)
    {
        string arg = info.GetArg(1);

        if (!ValidMaps.Contains(arg))
        {
            if (Config.WorkshopMapName.TryGetValue(arg, out ulong workshopMapId))
            {
                ExecuteMapCommand(player, arg, $"host_workshop_map {workshopMapId}", true);
                return;
            }

            SendMessageToReplyToCommand(info, "Map was not found", arg);
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
        string arg = info.GetArg(1);
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
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        ConVar? cvar = ConVar.Find(args[0]);

        if (cvar == null)
        {
            SendMessageToReplyToCommand(info, "Cvar is not found", args[0]);
            return;
        }

        if (cvar.Name.Equals("sv_cheats") && !AdminManager.PlayerHasPermissions(player, "@css/cheats"))
        {
            SendMessageToReplyToCommand(info, "You don't have permissions to change sv_cheats");
            return;
        }

        string value;

        if (info.ArgCount < 3)
        {
            value = GetCvarStringValue(cvar);

            SendMessageToReplyToCommand(info, "Cvar value", cvar.Name, value);
            return;
        }

        value = string.Join(' ', args[1..]);

        Server.ExecuteCommand($"{args[0]} {value}");

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
        Action<string> targetConsolePrint = player != null ? player.PrintToConsole : Server.PrintToConsole;

        if (info.ArgString.Length > 0)
        {
            TargetResult targetResult = info.GetArgTargetResult(1);

            if (targetResult.Players.Count == 0)
            {
                SendMessageToReplyToCommand(info, "No matching client");
                return;
            }
            else if (targetResult.Players.Count > 1)
            {
                SendMessageToReplyToCommand(info, "More than one client matched");
                return;
            }

            CCSPlayerController target = targetResult.Players[0];

            if (!AdminManager.CanPlayerTarget(player, target))
            {
                SendMessageToReplyToCommand(info, "Unable to target");
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
            SendMessageToAllPlayers(HudDestination.Chat, "Changing wsmap", adminname, map);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "Changing map", adminname, map);
        }
    }

    public void PrintPlayerInfo(Action<string> printer, CCSPlayerController player, string targetname, bool singletarget)
    {
        AdminData? data = AdminManager.GetPlayerAdminData(player);

        string permissionflags = data == null ? "none" :
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
}