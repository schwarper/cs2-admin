using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;

namespace Admin;

public partial class Admin : BasePlugin
{
    [ConsoleCommand("css_mute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Mute(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string targetname) = FindTarget(player, command, 1, false, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.VoiceFlags = VoiceFlags.Muted;
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_mute<player>", GetPlayerNameOrConsole(player), targetname);
        }
        else
        {
            PrintToChatAll("css_mute<multiple>", GetPlayerNameOrConsole(player), targetname);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_mute <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_unmute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_UnMute(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string targetname) = FindTarget(player, command, 1, false, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.VoiceFlags = VoiceFlags.Normal;
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_unmute<player>", GetPlayerNameOrConsole(player), targetname);
        }
        else
        {
            PrintToChatAll("css_unmute<multiple>", GetPlayerNameOrConsole(player), targetname);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_unmute <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_gag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Gag(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string targetname) = FindTarget(player, command, 1, false, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            SetPunishmentForPlayer(player, target, "gag", "", -1, false);
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_gag<player>", GetPlayerNameOrConsole(player), targetname);
        }
        else
        {
            PrintToChatAll("css_gag<multiple>", GetPlayerNameOrConsole(player), targetname);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_gag <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_ungag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_UnGag(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string targetname) = FindTarget(player, command, 1, false, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            RemovePunishment(target.SteamID, "gag", false);
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_ungag<player>", GetPlayerNameOrConsole(player), targetname);
        }
        else
        {
            PrintToChatAll("css_ungag<multiple>", GetPlayerNameOrConsole(player), targetname);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_ungag <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_silence")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Silence(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string targetname) = FindTarget(player, command, 1, false, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.VoiceFlags = VoiceFlags.Muted;

            SetPunishmentForPlayer(player, target, "gag", "", -1, false);
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_silence<player>", GetPlayerNameOrConsole(player), targetname);
        }
        else
        {
            PrintToChatAll("css_silence<multiple>", GetPlayerNameOrConsole(player), targetname);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_silence <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_unsilence")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_UnSilence(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string targetname) = FindTarget(player, command, 1, false, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.VoiceFlags = VoiceFlags.Normal;

            RemovePunishment(target.SteamID, "gag", false);
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_unsilence<player>", GetPlayerNameOrConsole(player), targetname);
        }
        else
        {
            PrintToChatAll("css_unsilence<multiple>", GetPlayerNameOrConsole(player), targetname);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_unsilence <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_smute")]
    [ConsoleCommand("css_tmute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 2, "<#userid|name> <time> - Imposes a timed mute", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TMute(CCSPlayerController? player, CommandInfo command)
    {
        HandleTGagMuteSilence(player, command, "mute", "css_smute", 2);
    }

    [ConsoleCommand("css_sunmute")]
    [ConsoleCommand("css_tunmute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name> - Unmute timed mute", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TUnMute(CCSPlayerController? player, CommandInfo command)
    {
        HandleTUnGagMuteSilence(player, command, "mute", "css_sunmute", 1);
    }

    [ConsoleCommand("css_sgag")]
    [ConsoleCommand("css_tgag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 2, "<#userid|name> <time> - Imposes a timed gag", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TGag(CCSPlayerController? player, CommandInfo command)
    {
        HandleTGagMuteSilence(player, command, "gag", "css_sgag", 2);
    }

    [ConsoleCommand("css_sungag")]
    [ConsoleCommand("css_tungag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name> - Ungag timed gag", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TUnGag(CCSPlayerController? player, CommandInfo command)
    {
        HandleTUnGagMuteSilence(player, command, "gag", "css_sungag", 1);
    }
    public void HandleTGagMuteSilence(CCSPlayerController? player, CommandInfo command, string punishment, string localizer, int argcount)
    {
        (List<CCSPlayerController> players, string targetname) = FindTarget(player, command, argcount, true, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        CCSPlayerController target = players.Single();

        if (!int.TryParse(command.GetArg(2), out int time))
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["Must be an integer"]);
            return;
        }

        SetPunishmentForPlayer(player, target, punishment, "", time, true);

        PrintToChatAll(localizer, GetPlayerNameOrConsole(player), targetname, time);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> {localizer} <{targetname}> <time>");
    }
    public void HandleTUnGagMuteSilence(CCSPlayerController? player, CommandInfo command, string punishment, string localizer, int argcount)
    {
        (List<CCSPlayerController> players, string targetname) = FindTarget(player, command, argcount, true, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        CCSPlayerController target = players.Single();

        RemovePunishment(target.SteamID, punishment, true);

        PrintToChatAll(localizer, GetPlayerNameOrConsole(player), targetname);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> {localizer} <{targetname}>");
    }
}