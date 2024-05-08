using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using static Admin.FindTarget;
using static Admin.Library;

namespace Admin;

public partial class Admin : BasePlugin
{
    [ConsoleCommand("css_slap")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands> <damage>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Slap(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string targetname) = Find(player, command, 1, false, true, MultipleFlags.IGNORE_DEAD_PLAYERS);

        if (players.Count == 0)
        {
            return;
        }

        if (!int.TryParse(command.GetArg(2), out int damage))
        {
            damage = 0;
        }

        foreach (CBasePlayerPawn? targetPawn in players.Select(p => p.Pawn.Value))
        {
            if (targetPawn == null)
            {
                continue;
            }

            targetPawn.Slap(damage);
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_slap<player>", player?.PlayerName ?? "Console", targetname, damage);
        }
        else
        {
            PrintToChatAll("css_slap<multiple>", player?.PlayerName ?? "Console", targetname, damage);
        }

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {player?.PlayerName ?? "Console"} -> css_slap <{command.GetArg(1)}> <{damage}>");
    }

    [ConsoleCommand("css_slay")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Slay(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string targetname) = Find(player, command, 1, false, true, MultipleFlags.IGNORE_DEAD_PLAYERS);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.CommitSuicide(false, true);
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_slay<player>", player?.PlayerName ?? "Console", targetname);
        }
        else
        {
            PrintToChatAll("css_slay<player>", player?.PlayerName ?? "Console", targetname);
        }

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {player?.PlayerName ?? "Console"} -> css_slay <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_rename")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 2, "<#userid|name> <newname>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_ReName(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string targetname) = Find(player, command, 2, true, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        CCSPlayerController target = players.Single();

        string newname = command.GetArg(2);

        if (string.IsNullOrEmpty(newname))
        {
            command.ReplyToCommand(Config.Tag + Localizer["Must be a string"]);
            return;
        }

        PrintToChatAll("css_rename", player?.PlayerName ?? "Console", targetname, newname);

        target.Rename(newname);

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {player?.PlayerName ?? "Console"} -> css_rename <{targetname}> <{newname}>");
    }
}