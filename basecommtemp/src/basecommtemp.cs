using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using static BaseCommTemp.Library;

namespace BaseCommTemp;

public class BaseCommTemp : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Basic Temp Comm Control";
    public override string ModuleVersion => "1.9";
    public override string ModuleAuthor => "schwarper";
    public override string ModuleDescription => "Provides methods of controlling communication.";

    public class PunishInfo
    {
        public ulong SteamID { get; set; }
        public string PunishName { get; set; } = string.Empty;
        public int Duration { get; set; }
        public DateTime Created { get; set; }
        public DateTime End { get; set; }
    }

    public static BaseCommTemp Instance { get; set; } = new BaseCommTemp();
    public static List<PunishInfo> PlayerTemporaryPunishList { get; set; } = [];
    public Config Config { get; set; } = new Config();

    public override void Load(bool hotReload)
    {
        Instance = this;

        AddCommandListener("say", Command_Say, HookMode.Pre);
        AddCommandListener("say_team", Command_SayTeam, HookMode.Pre);

        if (hotReload)
        {
            for (int i = 0; i < Server.MaxPlayers; i++)
            {
                CCSPlayerController? player = Utilities.GetEntityFromIndex<CCSPlayerController>(i + 1);

                if (player?.IsValid is not true || player.IsBot || player.DesignerName != playerdesignername || player.Connected != PlayerConnectedState.PlayerConnected)
                {
                    continue;
                }

                ulong steamid = player.SteamID;
                Task.Run(() => Database.LoadPlayer(steamid));
            }
        }

        AddTimer(60.0f, async () =>
        {
            PlayerTemporaryPunishList.RemoveAll(p => p.End <= DateTime.Now);
            await Database.UpdatePunishs();
        }, TimerFlags.REPEAT);
    }

    public override void Unload(bool hotReload)
    {
        RemoveCommandListener("say", Command_Say, HookMode.Pre);
        RemoveCommandListener("say_team", Command_SayTeam, HookMode.Pre);
    }

    public async void OnConfigParsed(Config config)
    {
        await Database.CreateDatabaseAsync(config);

        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);
        Config = config;
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player)
        {
            return HookResult.Continue;
        }

        ulong steamid = player.SteamID;
        Task.Run(() => Database.LoadPlayer(steamid));
        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        if (@event.Userid is not CCSPlayerController player)
        {
            return HookResult.Continue;
        }

        PlayerTemporaryPunishList.RemoveAll(p => p.SteamID == player.SteamID);
        return HookResult.Continue;
    }

    [ConsoleCommand("css_smute")]
    [ConsoleCommand("css_tmute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 2, "<#userid|name> <time> <reason> - Imposes a timed mute", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TMute(CCSPlayerController? player, CommandInfo info)
    {
        HandlePunish(player, info, "MUTE", "Mute");
    }

    [ConsoleCommand("css_sunmute")]
    [ConsoleCommand("css_tunmute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name> - Unmute timed mute", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TUnMute(CCSPlayerController? player, CommandInfo info)
    {
        HandleUnpunish(player, info, "MUTE", "UNMUTED", "Unmute");
    }

    [ConsoleCommand("css_sgag")]
    [ConsoleCommand("css_tgag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 2, "<#userid|name> <time> - Imposes a timed gag", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TGag(CCSPlayerController? player, CommandInfo info)
    {
        HandlePunish(player, info, "GAG", "Gag");
    }

    [ConsoleCommand("css_sungag")]
    [ConsoleCommand("css_tungag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<#userid|name> - Ungag timed gag", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TUnGag(CCSPlayerController? player, CommandInfo info)
    {
        HandleUnpunish(player, info, "GAG", "UNGAGGED", "Ungag");
    }

    public static void HandlePunish(CCSPlayerController? player, CommandInfo info, string punishment, string localizer)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        CCSPlayerController target = players[0];

        if (Database.IsPunished(target.SteamID, punishment))
        {
            SendMessageToReplyToCommand(info, $"Is already punished {localizer}", targetname);
            return;
        }

        if (!int.TryParse(args[1], out int duration) || duration < 0)
        {
            duration = 0;
        }

        ulong adminsteamid = player?.SteamID ?? 0;
        string reason = string.Join(' ', args[2..]);

        if (duration == 0)
        {
            if (string.IsNullOrEmpty(reason))
            {
                SendMessageToAllPlayers(HudDestination.Chat, $"Perma {localizer}", adminname, targetname, duration);
            }
            else
            {
                SendMessageToAllPlayers(HudDestination.Chat, $"Perma {localizer} reason", adminname, targetname, duration, reason);
            }
        }
        else
        {
            if (string.IsNullOrEmpty(reason))
            {
                SendMessageToAllPlayers(HudDestination.Chat, $"{localizer}", adminname, targetname, duration);
            }
            else
            {
                SendMessageToAllPlayers(HudDestination.Chat, $"{localizer} reason", adminname, targetname, duration, reason);
            }
        }

        DateTime created = DateTime.Now;
        DateTime end = created.AddMinutes(duration);

        switch (punishment)
        {
            case "GAG":
                {
                    PlayerTemporaryPunishList.Add(new PunishInfo()
                    {
                        SteamID = target.SteamID,
                        PunishName = punishment,
                        Created = created,
                        End = end,
                        Duration = duration
                    });
                    break;
                }
            case "MUTE":
                {
                    target.VoiceFlags = VoiceFlags.Muted;
                    break;
                }
        }

        Task.Run(() => Database.Punish(targetname, target.SteamID, adminname, adminsteamid, reason, duration, punishment));
    }

    public static void HandleUnpunish(CCSPlayerController? player, CommandInfo info, string dbpunishment, string punishment, string localizer)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        CCSPlayerController target = players[0];

        if (!Database.IsPunished(target.SteamID, dbpunishment))
        {
            SendMessageToReplyToCommand(info, $"Is not already punished {localizer}", targetname);
            return;
        }

        PlayerTemporaryPunishList.RemoveAll(p => p.SteamID == target.SteamID && p.PunishName == dbpunishment);

        SendMessageToAllPlayers(HudDestination.Chat, localizer, adminname, targetname);

        if (dbpunishment == "MUTE")
        {
            target.VoiceFlags = VoiceFlags.Normal;
        }

        Task.Run(() => Database.UnPunish(target.SteamID, dbpunishment, punishment));
    }

    private HookResult Command_Say(CCSPlayerController? player, CommandInfo info)
    {
        return Command_Say_Handler(player, info);
    }

    private HookResult Command_SayTeam(CCSPlayerController? player, CommandInfo info)
    {
        return Command_Say_Handler(player, info);
    }

    public static HookResult Command_Say_Handler(CCSPlayerController? player, CommandInfo info)
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

        PunishInfo? punish = PlayerTemporaryPunishList.FirstOrDefault(p => p.SteamID == player.SteamID && p.PunishName == "GAG");

        if (punish == null)
        {
            return HookResult.Continue;
        }

        SendMessageToPlayer(player, HudDestination.Chat, "You are gagged temp", punish.Duration, punish.Created, punish.End);
        return HookResult.Stop;
    }
}