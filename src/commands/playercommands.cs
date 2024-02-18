using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace Admin;

public partial class Admin : BasePlugin
{
    [ConsoleCommand("css_slap")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands> <damage>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Slap(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.IGNORE_DEAD_PLAYERS, 1);

        if (players == null)
        {
            return;
        }

        if (!int.TryParse(command.GetArg(2), out int damage))
        {
            damage = 0;
        }

        foreach (CBasePlayerPawn? targetPawn in players.Players.Select(p => p.Pawn.Value))
        {
            if (targetPawn == null)
            {
                continue;
            }

            targetPawn.Slap(damage);
        }

        if (players.Players.Length == 1)
        {
            PrintToChatAll("css_slap<player>", GetPlayerNameOrConsole(player), players.TargetName, damage);
        }
        else
        {
            PrintToChatAll("css_slap<multiple>", GetPlayerNameOrConsole(player), players.TargetName, damage);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_slap <{command.GetArg(1)}> <{damage}>");
    }

    [ConsoleCommand("css_slay")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Slay(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.IGNORE_DEAD_PLAYERS, 1);

        if (players == null)
        {
            return;
        }

        foreach (CCSPlayerController target in players.Players)
        {
            target.CommitSuicide(false, true);
        }

        if (players.Players.Length == 1)
        {
            PrintToChatAll("css_slay<player>", GetPlayerNameOrConsole(player), players.TargetName);
        }
        else
        {
            PrintToChatAll("css_slay<player>", GetPlayerNameOrConsole(player), players.TargetName);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_slay <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_rename")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 2, "<#userid|name> <newname>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_ReName(CCSPlayerController? player, CommandInfo command)
    {
        CCSPlayerController? target = FindTarget(command, MultipleFlags.NORMAL, 2);

        if (target == null)
        {
            return;
        }

        string newname = command.GetArg(2);

        if (string.IsNullOrEmpty(newname))
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["Must be a string"]);
            return;
        }

        PrintToChatAll("css_rename", GetPlayerNameOrConsole(player), target.PlayerName, newname);

        target.Rename(newname);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_rename <{target.PlayerName}> <{newname}>");
    }
}