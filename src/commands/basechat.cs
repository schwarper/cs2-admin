using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace Admin;

public partial class Admin : BasePlugin
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

        Server.PrintToChatAll(Localizer["css_say", GetPlayerNameOrConsole(player), message]);
        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] [{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_say <{message}>");
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
            if (!target.Valid())
            {
                return;
            }

            target.PrintToCenter(Localizer["css_csay", GetPlayerNameOrConsole(player), message]);
        });

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_csay <{message}>");
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
            Localizer["css_csay", GetPlayerNameOrConsole(player), message],
            0, 0, 0, 0);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_csay <{message}>");
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

        foreach (CCSPlayerController target in Utilities.GetPlayers().Where(p => p.Valid() && AdminManager.PlayerHasPermissions(p, "@css/chat")))
        {
            target.PrintToChat(Localizer["css_asay", GetPlayerNameOrConsole(player), message]);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_asay <{message}>");
    }

    [ConsoleCommand("css_psay")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 2, "<#userid|name> <message> - sends private message", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_PSay(CCSPlayerController? player, CommandInfo command)
    {
        CCSPlayerController? target = FindTarget(command, MultipleFlags.NORMAL, 2);

        if (target == null)
        {
            return;
        }

        string arg = command.GetCommandString;
        string message = arg[arg.IndexOf(' ')..];

        command.ReplyToCommand(Localizer["css_psay", GetPlayerNameOrConsole(player), target.PlayerName, message]);
        target.PrintToChat(Localizer["css_psay", GetPlayerNameOrConsole(player), target.PlayerName, message]);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_psay <{target.PlayerName}> <{message}>");
    }
}