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
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 2, true, true, MultipleFlags.NORMAL);

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

        target.Ban(targetname, player, adminname, reason, time);

        Library.PrintToChatAll("css_ban", adminname, targetname, time, reason);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_ban <{targetname}> <{time}> <{reason}>");
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

        var adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        Task.Run(() => Database.Unban(steamId.SteamId64, player, adminname));

        Library.PrintToChatAll("css_unban", adminname, steamid);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_unban <{steamid}>");
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

        var adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        if (target != null)
        {
            string targetname = target.PlayerName;

            target.Ban(targetname, player, adminname, reason, time);

            Library.PrintToChatAll("css_ban", adminname, targetname, time, reason);
            Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_addban <{targetname}> <{time}> <{reason}>");
        }
        else
        {
            Task.Run(() => Database.Ban(steamId.SteamId64, player, adminname, reason, time));

            Library.PrintToChatAll("css_addban", adminname, steamId.SteamId64, time, reason);
            Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_addban <{steamId.SteamId64}> <{time}> <{reason}>");
        }
    }
}