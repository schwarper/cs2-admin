using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using static CounterStrikeSharp.API.Modules.Commands.Targeting.Target;

namespace BaseComm;

public class BaseComm : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Basic Comm Control";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "schwarper";

    public static HashSet<CCSPlayerController> PlayerGagList { get; set; } = [];
    public Config Config { get; set; } = new Config();

    public void OnConfigParsed(Config config)
    {
        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);
        Config = config;
    }

    [ConsoleCommand("css_mute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, usage: "<player> - Removes a player's ability to use voice.")]
    public void Command_Mute(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries); ;

        if (!ProcessTargetString(player, info, args[0], false, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.VoiceFlags = VoiceFlags.Muted;
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_mute<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_mute<multiple>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_gag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, usage: "<player> - Removes a player's ability to use chat.")]
    public void Command_Gag(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries); ;

        if (!ProcessTargetString(player, info, args[0], false, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            PlayerGagList.Add(target);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_gag<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_gag<multiple>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_silence")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, usage: "<player> - Removes a player's ability to use chat.")]
    public void Command_Silence(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries); ;

        if (!ProcessTargetString(player, info, args[0], false, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.VoiceFlags = VoiceFlags.Muted;
            PlayerGagList.Add(target);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_silence<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_silence<multiple>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_unmute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, usage: "<player> - Restores a player's ability to use voice.")]
    public void Command_Unmute(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries); ;

        if (!ProcessTargetString(player, info, args[0], false, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.VoiceFlags = VoiceFlags.Normal;
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_unmute<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_unmute<multiple>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_ungag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, usage: "<player> - Restores a player's ability to use chat.")]
    public void Command_Ungag(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries); ;

        if (!ProcessTargetString(player, info, args[0], false, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            PlayerGagList.Remove(target);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_ungag<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_ungag<multiple>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_unsilence")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, usage: "<player> - Restores a player's ability to use voice and chat.")]
    public void Command_Unsilence(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries); ;

        if (!ProcessTargetString(player, info, args[0], false, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.VoiceFlags = VoiceFlags.Normal;
            PlayerGagList.Remove(target);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_unsilence<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_unsilence<multiple>", adminname, targetname);
        }
    }

    [GameEventHandler]
    public HookResult OnRoundStart(EventPlayerSpawn @event, GameEventInfo info)
    {
        PlayerGagList.Clear();
        return HookResult.Continue;
    }

    private void SendMessageToAllPlayers(HudDestination destination, string messageKey, params object[] args)
    {
        Microsoft.Extensions.Localization.LocalizedString message = Localizer[messageKey, args];
        VirtualFunctions.ClientPrintAll(destination, Config.Tag + message, 0, 0, 0, 0);
    }

    private bool ProcessTargetString(
        CCSPlayerController? player,
        CommandInfo info,
        string targetstr,
        bool singletarget,
        out List<CCSPlayerController> players,
        out string adminname,
        out string targetname)
    {
        players = [];
        adminname = string.Empty;
        targetname = string.Empty;

        TargetResult targetResult = new Target(targetstr).GetTarget(player);

        if (targetResult.Players.Count == 0)
        {
            info.ReplyToCommand(Config.Tag + Localizer["No matching client"]);
            return false;
        }
        else if (targetResult.Players.Count > 1)
        {
            if (!TargetTypeMap.ContainsKey(targetstr) || singletarget)
            {
                info.ReplyToCommand(Config.Tag + Localizer["More than one client matched"]);
                return false;
            }

            targetResult.Players.RemoveAll(target => !AdminManager.CanPlayerTarget(player, target));

            if (targetResult.Players.Count == 0)
            {
                info.ReplyToCommand(Config.Tag + Localizer["Unable to targets"]);
                return false;
            }
        }
        else
        {
            CCSPlayerController target = targetResult.Players[0];

            if (!AdminManager.CanPlayerTarget(player, target))
            {
                info.ReplyToCommand(Config.Tag + Localizer["Unable to target"]);
                return false;
            }

            players = [target];
            adminname = player?.PlayerName ?? Localizer["Console"];
            targetname = target.PlayerName;
            return true;
        }

        TargetTypeMap.TryGetValue(targetstr, out TargetType type);

        adminname = player?.PlayerName ?? Localizer["Console"];
        targetname = type switch
        {
            TargetType.GroupAll => Localizer["all"],
            TargetType.GroupBots => Localizer["bots"],
            TargetType.GroupHumans => Localizer["humans"],
            TargetType.GroupAlive => Localizer["alive"],
            TargetType.GroupDead => Localizer["dead"],
            TargetType.GroupNotMe => Localizer["notme"],
            TargetType.PlayerMe => targetResult.Players[0].PlayerName,
            TargetType.TeamCt => Localizer["ct"],
            TargetType.TeamT => Localizer["t"],
            TargetType.TeamSpec => Localizer["spec"],
            _ => targetResult.Players[0].PlayerName
        };

        return true;
    }
}