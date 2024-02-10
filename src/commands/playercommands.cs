using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Timers;

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

        foreach (var targetPawn in players.Players.Select(p => p.Pawn.Value))
        {
            if (targetPawn == null)
            {
                continue;
            }

            targetPawn.Slap(damage);
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_slap<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, damage]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_slap<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, damage]);
        }
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

        foreach (var target in players.Players)
        {
            target.CommitSuicide(false, true);
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_slay<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_slay<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
    }

    [ConsoleCommand("css_rename")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 2, "<#userid|name> <newname>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_ReName(CCSPlayerController? player, CommandInfo command)
    {
        CCSPlayerController? target = FindTarget(command, 2);

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

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_rename", player == null ? Localizer["Console"] : player.PlayerName, target.PlayerName, newname]);

        target.Rename(newname);
    }
}