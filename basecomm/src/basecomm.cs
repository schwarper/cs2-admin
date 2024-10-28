using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using static BaseComm.Library;

namespace BaseComm;

public class BaseComm : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Basic Comm Control";
    public override string ModuleVersion => "1.6";
    public override string ModuleAuthor => "schwarper";
    public override string ModuleDescription => "Provides methods of controlling communication.";

    public static BaseComm Instance { get; set; } = new BaseComm();
    public static HashSet<CCSPlayerController> PlayerGagList { get; set; } = [];
    public Config Config { get; set; } = new Config();

    public override void Load(bool hotReload)
    {
        Instance = this;

        AddCommandListener("say", Command_Say, HookMode.Pre);
        AddCommandListener("say_team", Command_SayTeam, HookMode.Pre);
    }

    public override void Unload(bool hotReload)
    {
        RemoveCommandListener("say", Command_Say, HookMode.Pre);
        RemoveCommandListener("say_team", Command_SayTeam, HookMode.Pre);
    }

    public void OnConfigParsed(Config config)
    {
        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);
        Config = config;
    }

    private HookResult Command_Say(CCSPlayerController? player, CommandInfo info)
    {
        return Command_Say_Handler(player, info);
    }

    private HookResult Command_SayTeam(CCSPlayerController? player, CommandInfo info)
    {
        return Command_Say_Handler(player, info);
    }

    public HookResult Command_Say_Handler(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            return HookResult.Continue;
        }

        string arg = info.GetArg(1);

        if (CoreConfig.SilentChatTrigger.Any(i => arg.StartsWith(i)))
        {
            return HookResult.Continue;
        }

        if (PlayerGagList.Contains(player))
        {
            SendMessageToPlayer(player, HudDestination.Chat, "You are gagged");
            return HookResult.Stop;
        }

        return HookResult.Continue;
    }

    [ConsoleCommand("css_mute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, usage: "<#userid|name|all @ commands> - Removes a player's ability to use voice.")]
    public void Command_Mute(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
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
    [CommandHelper(minArgs: 1, usage: "<#userid|name|all @ commands> - Removes a player's ability to use chat.")]
    public void Command_Gag(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
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
    [CommandHelper(minArgs: 1, usage: "<#userid|name|all @ commands> - Removes a player's ability to use voice or chat.")]
    public void Command_Silence(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
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
    [CommandHelper(minArgs: 1, usage: "<#userid|name|all @ commands> - Restores a player's ability to use voice.")]
    public void Command_Unmute(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
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
    [CommandHelper(minArgs: 1, usage: "<#userid|name|all @ commands> - Restores a player's ability to use chat.")]
    public void Command_Ungag(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
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
    [CommandHelper(minArgs: 1, usage: "<#userid|name|all @ commands> - Restores a player's ability to use voice and chat.")]
    public void Command_Unsilence(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
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
}