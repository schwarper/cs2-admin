using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using static Admin.FindTarget;

namespace Admin;

public partial class Admin
{
    [ConsoleCommand("css_say")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<message> - sends message to all players", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Say(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 1)
        {
            return;
        }

        string arg = command.GetCommandString;
        string message = arg[arg.IndexOf(' ')..];

        Server.PrintToChatAll(Localizer["css_say", player?.PlayerName ?? "Console", message]);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {player?.PlayerName ?? "Console"} -> css_say <{message}>");
    }

    [ConsoleCommand("css_csay")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<message> - sends centered message to all players", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_CSay(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 1)
        {
            return;
        }

        string arg = command.GetCommandString;
        string message = arg[arg.IndexOf(' ')..];

        Utilities.GetPlayers().ForEach(target =>
        {
            target.PrintToCenter(Localizer["css_csay", player?.PlayerName ?? "Console", message]);
        });

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {player?.PlayerName ?? "Console"} -> css_csay <{message}>");
    }

    [ConsoleCommand("css_dsay")]
    [ConsoleCommand("css_hsay")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<message>  - sends hud message to all players", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_DSay(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 1)
        {
            return;
        }

        string arg = command.GetCommandString;
        string message = arg[arg.IndexOf(' ')..];

        VirtualFunctions.ClientPrintAll(HudDestination.Alert,
            Localizer["css_csay", player?.PlayerName ?? "Console", message],
            0, 0, 0, 0);

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {player?.PlayerName ?? "Console"} -> css_csay <{message}>");
    }

    [ConsoleCommand("css_asay")]
    [ConsoleCommand("css_chat")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<message> - sends message to admins", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_ASay(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 1)
        {
            return;
        }

        string arg = command.GetCommandString;
        string message = arg[arg.IndexOf(' ')..];

        foreach (CCSPlayerController target in Utilities.GetPlayers().Where(p => AdminManager.PlayerHasPermissions(p, "@css/chat")))
        {
            target.PrintToChat(Localizer["css_asay", player?.PlayerName ?? "Console", message]);
        }

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {player?.PlayerName ?? "Console"} -> css_asay <{message}>");
    }

    [ConsoleCommand("css_psay")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 2, "<#userid|name> <message> - sends public message", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_PSay(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string targetname) = Find(player, command, 2, true, false, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        CCSPlayerController target = players.Single();

        string arg = command.GetCommandString;
        string message = arg[arg.IndexOf(' ')..];

        command.ReplyToCommand(Localizer["css_psay", player?.PlayerName ?? "Console", targetname, message]);
        target.PrintToChat(Localizer["css_psay", player?.PlayerName ?? "Console", targetname, message]);

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {player?.PlayerName ?? "Console"} -> css_psay <{targetname}> <{message}>");
    }
}