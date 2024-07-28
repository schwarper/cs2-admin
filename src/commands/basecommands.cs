using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using static Admin.FindTarget;
using static Admin.Library;

namespace Admin;

public partial class Admin
{
    [ConsoleCommand("css_kick")]
    [RequiresPermissions("@css/kick")]
    [CommandHelper(minArgs: 1, "<#userid|name> <reason>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Kick(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, true, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        CCSPlayerController target = players.Single();

        if (!AdminManager.CanPlayerTarget(player, target))
        {
            command.ReplyToCommand(Config.Tag + Localizer["You cannot target"]);
            return;
        }

        string reason = command.GetArg(2);

        if (reason == string.Empty)
        {
            reason = Localizer["Unknown"];
        }

        target.Kick();

        PrintToChatAll("css_kick", adminname, targetname, reason);

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_kick <{targetname}> <{reason}>");
    }

    [ConsoleCommand("css_changemap")]
    [ConsoleCommand("css_map")]
    [RequiresPermissions("@css/changemap")]
    [CommandHelper(minArgs: 1, "<map>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Map(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 1)
            return;

        string map = command.GetArg(1);

        if (!Server.IsMapValid(map))
        {
            if (Config.WorkshopMapName.TryGetValue(map, out ulong workshopMapId))
            {
                ExecuteMapCommand($"host_workshop_map {workshopMapId}", player, map);
                return;
            }

            command.ReplyToCommand(Config.Tag + Localizer["Map is not exist"]);
            return;
        }

        ExecuteMapCommand($"changelevel {map}", player, map);
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
        string mapCommand;

        if (!ulong.TryParse(map, out ulong workshopMapId))
        {
            mapCommand = $"ds_workshop_changelevel {map}";
        }
        else
        {
            string workshopName = Config.WorkshopMapName.FirstOrDefault(p => p.Value == workshopMapId).Key;

            if (workshopName == null)
            {
                mapCommand = $"host_workshop_map {map}";
            }
            else
            {
                mapCommand = $"host_workshop_map {workshopMapId}";
                map = workshopName;
            }
        }

        ExecuteMapCommand(mapCommand, player, map);
    }

    private void ExecuteMapCommand(string mapCommand, CCSPlayerController? player, string map)
    {
        var adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        PrintToChatAll(mapCommand.Contains("workshop") ? "css_wsmap" : "css_map", adminname, map);

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> {mapCommand}");

        AddTimer(Config.ChangeMapDelay, () =>
        {
            Server.ExecuteCommand(mapCommand);
        });
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

        var adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        string arg = command.ArgString;

        Server.ExecuteCommand(arg);

        PrintToChatAll("css_rcon", adminname, arg);

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_rcon <{arg}>");
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
            command.ReplyToCommand(Config.Tag + Localizer["Cvar is not found", cvarname]);
            return;
        }

        if (cvar.Name.Equals("sv_cheats") && !AdminManager.PlayerHasPermissions(player, "@css/cheats"))
        {
            command.ReplyToCommand(Config.Tag + Localizer["You don't have permissions to change sv_cheats"]);
            return;
        }

        string value;

        if (command.ArgCount < 3)
        {
            value = GetCvarStringValue(cvar);

            command.ReplyToCommand(Config.Tag + Localizer["Cvar value", cvar.Name, value]);
            return;
        }

        value = command.GetArg(2);

        Server.ExecuteCommand($"{cvarname} {value}");

        var adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        PrintToChatAll("css_cvar", adminname, cvar.Name, value);

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_cvar <{cvar.Name}> <{value}");
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

        var adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        PrintToChatAll("css_exec", adminname, cfg);

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_exec <{cfg}>");
    }

    [ConsoleCommand("css_rr")]
    [RequiresPermissions("@css/root")]
    public void Command_RestartRound(CCSPlayerController? player, CommandInfo command)
    {
        Server.ExecuteCommand("mp_restartgame 2");

        var adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        PrintToChatAll("css_rr", adminname);

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_rr");
    }

    [ConsoleCommand("css_who")]
    [RequiresPermissions("@css/generic")]
    [CommandHelper(minArgs: 0, "<#userid|name or empty for all>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Who(CCSPlayerController? player, CommandInfo command)
    {
        Action<string> targetConsolePrint = (player != null) ? player.PrintToConsole : Server.PrintToConsole;

        if (command.ArgCount > 1)
        {
            (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, true, true, MultipleFlags.NORMAL);

            if (players.Count == 0)
            {
                return;
            }

            CCSPlayerController target = players.Single();

            PrintPlayerInfo(targetConsolePrint, target, targetname, true);

            Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_who <{targetname}>");
        }
        else
        {
            foreach (CCSPlayerController target in Utilities.GetPlayers().Where(target => AdminManager.CanPlayerTarget(player, target)))
            {
                PrintPlayerInfo(targetConsolePrint, target, string.Empty, false);
            }

            var adminname = player?.PlayerName ?? Instance.Localizer["Console"];

            Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_who");
        }
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
}