using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using static Admin.FindTarget;

namespace Admin;

public partial class Admin
{
    [ConsoleCommand("css_ban")]
    [RequiresPermissions("@css/ban")]
    [CommandHelper(minArgs: 2, "<#userid|name> <time> <reason>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Ban(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string targetname) = Find(player, command, 2, true, true, MultipleFlags.NORMAL);

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

        if (!int.TryParse(command.GetArg(2), out int time))
        {
            command.ReplyToCommand(Config.Tag + Localizer["Must be an integer"]);
            return;
        }

        string reason = command.GetArg(3) ?? Localizer["Unknown"];

        string playerName = player?.PlayerName ?? "Console";

        Task.Run(() => Database.Ban(target, targetname, player, playerName, reason, time));

        target.Kick();

        Library.PrintToChatAll("css_ban", playerName, targetname, time, reason);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {playerName} -> css_ban <{targetname}> <{time}> <{reason}>");
    }

    [ConsoleCommand("css_unban")]
    [RequiresPermissions("@css/ban")]
    [CommandHelper(minArgs: 1, "<steamid>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_UnBan(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 1)
        {
            return;
        }

        string steamid = command.GetArg(1);

        if (!SteamID.TryParse(steamid, out SteamID? steamId) || steamId == null)
        {
            command.ReplyToCommand(Config.Tag + Localizer["Must be a steamid"]);
            return;
        }

        string playerName = player?.PlayerName ?? "Console";

        Task.Run(() => Database.Unban(steamId.SteamId64, player, playerName));

        Library.PrintToChatAll("css_unban", playerName, steamid);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {playerName} -> css_unban <{steamid}>");
    }

    [ConsoleCommand("css_addban")]
    [RequiresPermissions("@css/ban")]
    [CommandHelper(minArgs: 2, "<steamid> <time> <reason>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Addban(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 2)
        {
            return;
        }

        string steamid = command.GetArg(1);

        if (!SteamID.TryParse(steamid, out SteamID? steamId) || steamId == null)
        {
            command.ReplyToCommand(Config.Tag + Localizer["Must be a steamid"]);
            return;
        }

        if (AdminManager.GetPlayerImmunity(player) < AdminManager.GetPlayerImmunity(steamId))
        {
            command.ReplyToCommand(Config.Tag + Localizer["You cannot target"]);
            return;
        }

        if (!int.TryParse(command.GetArg(2), out int time))
        {
            command.ReplyToCommand(Config.Tag + Localizer["Must be an integer"]);
            return;
        }

        string reason = command.GetArg(3) ?? Localizer["Unknown"];

        CCSPlayerController? target = Utilities.GetPlayerFromSteamId(steamId.SteamId64);

        string playerName = player?.PlayerName ?? "Console";

        if (target != null)
        {
            string targetname = target.PlayerName;

            Task.Run(() => Database.Ban(target, targetname, player, playerName, reason, time));
            target.Kick();

            Library.PrintToChatAll("css_ban", playerName, targetname, time, reason);
            Discord.SendMessage($"[{player?.SteamID ?? 0}] {playerName} -> css_addban <{targetname}> <{time}> <{reason}>");
        }
        else
        {
            Task.Run(() => Database.Ban(steamId.SteamId64, player, playerName, reason, time));

            Library.PrintToChatAll("css_addban", playerName, steamId.SteamId64, time, reason);
            Discord.SendMessage($"[{player?.SteamID ?? 0}] {playerName} -> css_addban <{steamId.SteamId64}> <{time}> <{reason}>");
        }
    }
}