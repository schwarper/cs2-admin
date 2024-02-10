using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API;

namespace Admin;

public partial class Admin : BasePlugin
{
    [ConsoleCommand("css_mute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Mute(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.NORMAL, 1);

        if (players == null)
        {
            return;
        }

        foreach (var target in players.Players)
        {
            target.VoiceFlags = VoiceFlags.Muted;
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_mute<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_mute<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
    }

    [ConsoleCommand("css_unmute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_UnMute(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.NORMAL, 1);

        if (players == null)
        {
            return;
        }

        foreach (var target in players.Players)
        {
            target.VoiceFlags = VoiceFlags.Normal;
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_unmute<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_unmute<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
    }

    [ConsoleCommand("css_gag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Gag(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.NORMAL, 1);

        if (players == null)
        {
            return;
        }

        foreach (var target in players.Players)
        {
            SetPunishmentForPlayer(player, target, "gag", "", -1, false);
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_gag<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_gag<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
    }

    [ConsoleCommand("css_ungag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_UnGag(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.NORMAL, 1);

        if (players == null)
        {
            return;
        }

        foreach (var target in players.Players)
        {
            RemovePunishment(target.SteamID, "gag", false);
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_ungag<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_ungag<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
    }

    [ConsoleCommand("css_silence")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Silence(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.NORMAL, 1);

        if (players == null)
        {
            return;
        }

        foreach (var target in players.Players)
        {
            target.VoiceFlags = VoiceFlags.Muted;

            SetPunishmentForPlayer(player, target, "gag", "", -1, false);
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_silence<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_silence<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
    }

    [ConsoleCommand("css_unsilence")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_UnSilence(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.NORMAL, 1);

        if (players == null)
        {
            return;
        }

        foreach (var target in players.Players)
        {
            target.VoiceFlags = VoiceFlags.Normal;

            RemovePunishment(target.SteamID, "gag", false);
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_unsilence<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_unsilence<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
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
        CCSPlayerController? target = FindTarget(command, argcount);

        if (target == null)
        {
            return;
        }

        if(!int.TryParse(command.GetArg(2), out int time))
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["Must be an integer"]);
            return;
        }

        SetPunishmentForPlayer(player, target, punishment, "", time, true);

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer[localizer, player == null ? Localizer["Console"] : player.PlayerName, target.PlayerName, time]);
    }
    public void HandleTUnGagMuteSilence(CCSPlayerController? player, CommandInfo command, string punishment, string localizer, int argcount)
    {
        CCSPlayerController? target = FindTarget(command, argcount);

        if (target == null)
        {
            return;
        }

        RemovePunishment(target.SteamID, punishment, true);

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer[localizer, player == null ? Localizer["Console"] : player.PlayerName, target.PlayerName]);
    }
}