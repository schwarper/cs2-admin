using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Collections.Concurrent;
using System.Reflection.Metadata;

namespace AntiFlood;

public class AntiFlood : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Anti-Flood";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "schwarper";
    public override string ModuleDescription => "Protects against chat flooding";

    public class PlayerInfo
    {
        public float lastduration;
        public int tokenCount;
    }

    public Config Config { get; set; } = new Config();
    public ConcurrentDictionary<ulong, PlayerInfo> playerInfo = [];
    public FakeConVar<float> css_flood_duration = new("css_flood_duration", "Amount of duration allowed between chat messages", 0.75f);

    public override void Load(bool hotReload)
    {
        AddCommandListener("say", Command_Say, HookMode.Pre);
        AddCommandListener("say_team", Command_Say, HookMode.Pre);
    }

    public override void Unload(bool hotReload)
    {
        RemoveCommandListener("say", Command_Say, HookMode.Pre);
        RemoveCommandListener("say_team", Command_Say, HookMode.Pre);
    }

    public void OnConfigParsed(Config config)
    {
        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);
        Config = config;
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player?.IsBot ?? true)
        {
            return HookResult.Continue;
        }

        playerInfo.TryAdd(player.SteamID, new PlayerInfo());
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player?.IsBot ?? true)
        {
            return HookResult.Continue;
        }

        playerInfo.TryRemove(player.SteamID, out _);
        return HookResult.Continue;
    }

    public HookResult Command_Say(CCSPlayerController? player, CommandInfo info)
    {
        if (css_flood_duration.Value <= 0 || AdminManager.PlayerHasPermissions(player, "@css/chat"))
        {
            return HookResult.Continue;
        }

        float curduration = Server.CurrentTime;
        float newduration = curduration + css_flood_duration.Value;
        PlayerInfo playerData = playerInfo[player!.SteamID];

        if (playerData.lastduration >= curduration)
        {
            if (playerData.tokenCount >= 3)
            {
                VirtualFunctions.ClientPrint(player.Handle, HudDestination.Chat, Config.Tag + Localizer["Flooding the server"], 0, 0, 0, 0);
                playerData.lastduration = newduration + 3.0f;
                return HookResult.Handled;
            }
            else
            {
                playerData.tokenCount++;
            }
        }
        else if (playerData.tokenCount > 0)
        {
            playerData.tokenCount--;
        }

        playerData.lastduration = newduration;
        return HookResult.Continue;
    }
}