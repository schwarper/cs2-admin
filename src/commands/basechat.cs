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

        var adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        Server.PrintToChatAll(Localizer["css_say", adminname, message]);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_say <{message}>");
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

        var adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        Utilities.GetPlayers().ForEach(target =>
        {
            target.PrintToCenter(Localizer["css_csay", adminname, message]);
        });

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_csay <{message}>");
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

        var adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        VirtualFunctions.ClientPrintAll(HudDestination.Alert,
            Localizer["css_csay", adminname, message],
            0, 0, 0, 0);

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_csay <{message}>");
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

        var adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        foreach (CCSPlayerController target in Utilities.GetPlayers().Where(p => AdminManager.PlayerHasPermissions(p, "@css/chat")))
        {
            target.PrintToChat(Localizer["css_asay", adminname, message]);
        }

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_asay <{message}>");
    }

    [ConsoleCommand("css_psay")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 2, "<#userid|name> <message> - sends public message", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_PSay(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 2, true, false, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        CCSPlayerController target = players.Single();

        string arg = command.GetCommandString;
        string message = arg[arg.IndexOf(' ')..];

        command.ReplyToCommand(Localizer["css_psay", adminname, targetname, message]);
        target.PrintToChat(Localizer["css_psay", adminname, targetname, message]);

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_psay <{targetname}> <{message}>");
    }
}