using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;

namespace Admin;

public partial class Admin : BasePlugin
{
    [ConsoleCommand("css_kick")]
    [RequiresPermissions("@css/kick")]
    [CommandHelper(minArgs: 1, "<#userid|name> <reason>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Kick(CCSPlayerController? player, CommandInfo command)
    {
        CCSPlayerController? target = FindTarget(command, MultipleFlags.NORMAL, 1);

        if (target == null)
        {
            return;
        }

        if (!AdminManager.CanPlayerTarget(player, target))
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["You cannot target"]);
            return;
        }

        string reason = command.GetArg(2) ?? Localizer["Unknown"];

        KickPlayer(target, reason);

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_kick", GetPlayerNameOrConsole(player), target.PlayerName, reason]);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_kick <{target.PlayerName}> <{reason}>");
    }

    [ConsoleCommand("css_changemap")]
    [ConsoleCommand("css_map")]
    [RequiresPermissions("@css/changemap")]
    [CommandHelper(minArgs: 1, "<map>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Map(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 1)
        {
            return;
        }

        string map = command.GetArg(1);

        if (map.StartsWith("ws:"))
        {
            string mapname;

            if (long.TryParse(map, out long mapId))
            {
                mapname = $"host_workshop_map {mapId}";
            }
            else
            {
                mapname = $"ds_workshop_changelevel {map}";
            }

            AddTimer(Config.ChangeMapDelay, () =>
            {
                Server.ExecuteCommand(mapname);
            });

            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_wsmap", GetPlayerNameOrConsole(player), map]);

            _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_wsmap <{map}>");

            return;
        }

        if (!Server.IsMapValid(map))
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["Map is not exist"]);
            return;
        }

        AddTimer(Config.ChangeMapDelay, () =>
        {
            Server.ExecuteCommand($"changelevel {map}");
        });

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_map", GetPlayerNameOrConsole(player), map]);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_map <{map}>");
    }

    [ConsoleCommand("css_wsmap")]
    [ConsoleCommand("css_workshop")]
    [RequiresPermissions("@css/changemap")]
    [CommandHelper(minArgs: 1, "<workshop map>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_WorkshopMap(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 1)
        {
            return;
        }

        string map = command.GetArg(1);
        string mapname;

        if (long.TryParse(map, out long mapId))
        {
            mapname = $"host_workshop_map {mapId}";
        }
        else
        {
            mapname = $"ds_workshop_changelevel {map}";
        }

        AddTimer(Config.ChangeMapDelay, () =>
        {
            Server.ExecuteCommand(mapname);
        });

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_wsmap", GetPlayerNameOrConsole(player), map]);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_wsmap <{map}>");
    }

    [ConsoleCommand("css_rcon")]
    [RequiresPermissions("@css/rcon")]
    [CommandHelper(minArgs: 1, "<args>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Rcon(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 1)
        {
            return;
        }

        string arg = command.ArgString;

        Server.ExecuteCommand(arg);

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_rcon", GetPlayerNameOrConsole(player), arg]);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_rcon <{arg}>");
    }

    [ConsoleCommand("css_cvar")]
    [RequiresPermissions("@css/cvar")]
    [CommandHelper(minArgs: 1, "<cvar> <value>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Cvar(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 1)
        {
            return;
        }

        string cvarname = command.GetArg(1);

        ConVar? cvar = ConVar.Find(cvarname);

        if (cvar == null)
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["Cvar is not found", cvarname]);
            return;
        }

        if (cvar.Name.Equals("sv_cheats") && !AdminManager.PlayerHasPermissions(player, "@css/cheats"))
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["You don't have permissions to change sv_cheats"]);
            return;
        }

        string value;

        if (command.ArgCount < 3)
        {
            value = GetCvarStringValue(cvar);

            command.ReplyToCommand(Localizer["Prefix"] + Localizer["Cvar value", cvar.Name, value]);
            return;
        }

        value = command.GetArg(2);

        Server.ExecuteCommand($"{cvarname} {value}");

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_cvar", GetPlayerNameOrConsole(player), cvar.Name, value]);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_cvar <{cvar.Name}> <{value}");
    }

    [ConsoleCommand("css_exec")]
    [RequiresPermissions("@css/config")]
    [CommandHelper(minArgs: 1, "<exec>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Exec(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 1)
        {
            return;
        }

        string cfg = command.ArgString;

        Server.ExecuteCommand($"exec {cfg}");

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_exec", GetPlayerNameOrConsole(player), cfg]);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_exec <{cfg}>");
    }

    [ConsoleCommand("css_who")]
    [RequiresPermissions("@css/generic")]
    [CommandHelper(minArgs: 1, "<#userid|name>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Who(CCSPlayerController? player, CommandInfo command)
    {
        CCSPlayerController? target = FindTarget(command, MultipleFlags.NORMAL, 1);

        if (target == null)
        {
            return;
        }

        AdminData? data = AdminManager.GetPlayerAdminData(target);

        string permissionflags = (data == null) ? "none" :
                          data.GetAllFlags().Contains("@css/root") ? "root" :
                          string.Join(",", data.GetAllFlags()).Replace("@css/", "");

        Server.NextFrame(() =>
        {
            var targetConsolePrint = (player != null) ? (Action<string>)player.PrintToConsole : Server.PrintToConsole;

            targetConsolePrint(Localizer["css_who<title>", target.PlayerName]);
            targetConsolePrint(Localizer["css_who<steamid>", target.SteamID]);
            targetConsolePrint(Localizer["css_who<ip>", target.IpAddress ?? Localizer["Unknown"]]);
            targetConsolePrint(Localizer["css_who<permission>", permissionflags]);
            targetConsolePrint(Localizer["css_who<immunitylevel>", AdminManager.GetPlayerImmunity(target)]);
        });

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_who <{target.PlayerName}>");
    }
}