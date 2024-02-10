using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Entities;

namespace Admin;

public partial class Admin : BasePlugin
{
    [ConsoleCommand("css_ban")]
    [RequiresPermissions("@css/ban")]
    [CommandHelper(minArgs: 2, "<#userid|name> <time> <reason>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Ban(CCSPlayerController? player, CommandInfo command)
    {
        CCSPlayerController? target = FindTarget(command, 2);

        if (target == null)
        {
            return;
        }

        if (!int.TryParse(command.GetArg(2), out int time))
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["Must be an integer"]);
            return;
        }

        string reason = command.GetArg(3) ?? Localizer["Unknown"];

        SetPunishmentForPlayer(player, target, "ban", reason, time, true);

        KickPlayer(target, reason);

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_ban", player == null ? Localizer["Console"] : player.PlayerName, target.PlayerName, time, reason]);
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

        var steamid = command.GetArg(1);

        if(!SteamID.TryParse(steamid, out SteamID? steamId) || steamId == null)
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["Must be a steamid"]);
            return;
        }

        RemovePunishment(steamId.SteamId64, "ban", true);

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_unban", player == null ? Localizer["Console"] : player.PlayerName, steamid]);
    }
}