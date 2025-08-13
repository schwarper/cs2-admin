using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using CounterStrikeSharp.API.ValveConstants.Protobuf;
using static BaseBans.Library;
using static CounterStrikeSharp.API.Core.Listeners;

namespace BaseBans;

public class BaseBans : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Basic Ban Commands";
    public override string ModuleVersion => "1.9";
    public override string ModuleAuthor => "schwarper";
    public override string ModuleDescription => "Basic Banning Commands";

    public static BaseBans Instance { get; set; } = new();
    public Config Config { get; set; } = new Config();

    public override void Load(bool hotReload)
    {
        Instance = this;

        RegisterListener<OnClientAuthorized>(OnClientAuthorized);
    }

    public override void Unload(bool hotReload)
    {
        RemoveListener<OnClientAuthorized>(OnClientAuthorized);
    }

    public async void OnConfigParsed(Config config)
    {
        await Database.CreateDatabaseAsync(config);

        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);
        Config = config;
    }

    public static async void OnClientAuthorized(int playerSlot, SteamID steamId)
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(playerSlot);

        if (player == null || player.IsBot)
        {
            return;
        }

        if (await Database.IsBannedAsync(steamId.SteamId64))
        {
            Server.NextFrame(() => player.Disconnect(NetworkDisconnectionReason.NETWORK_DISCONNECT_STEAM_BANNED));
        }
    }

    [ConsoleCommand("css_ban")]
    [RequiresPermissions("@css/ban")]
    [CommandHelper(minArgs: 2, usage: "<#userid|name> <minutes|0> [reason]")]
    public void Command_Ban(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
        {
            return;
        }

        CCSPlayerController target = players[0];

        if (!AdminManager.CanPlayerTarget(player, target))
        {
            SendMessageToReplyToCommand(info, "Unable to target");
            return;
        }

        if (Database.IsBanned(target.SteamID))
        {
            SendMessageToReplyToCommand(info, "Already banned", targetname);
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
                SendMessageToAllPlayers(HudDestination.Chat, "Permabanned", adminname, targetname, duration);
            }
            else
            {
                SendMessageToAllPlayers(HudDestination.Chat, "Permabanned reason", adminname, targetname, duration, reason);
            }
        }
        else
        {
            if (string.IsNullOrEmpty(reason))
            {
                SendMessageToAllPlayers(HudDestination.Chat, "Banned", adminname, targetname, duration);
            }
            else
            {
                SendMessageToAllPlayers(HudDestination.Chat, "Banned reason", adminname, targetname, duration, reason);
            }
        }

        Task.Run(async () =>
        {
            await Database.Ban(targetname, target.SteamID, adminname, adminsteamid, reason, duration);
            await Discord.SendEmbedMessage(targetname, target.SteamID, adminname, adminsteamid, reason, duration, true);
        });

        target.Disconnect(NetworkDisconnectionReason.NETWORK_DISCONNECT_STEAM_BANNED);
    }

    [ConsoleCommand("css_unban")]
    [RequiresPermissions("@css/unban")]
    [CommandHelper(minArgs: 1, usage: "<steamid>")]
    public void Command_Unban(CCSPlayerController? player, CommandInfo info)
    {
        if (!SteamIDTryParse(info.GetArg(1), out ulong steamid))
        {
            SendMessageToReplyToCommand(info, "Invalid SteamID specified");
            return;
        }

        if (!Database.IsBanned(steamid))
        {
            SendMessageToReplyToCommand(info, "Not already banned", steamid);
            return;
        }

        ulong adminsteamid = player?.SteamID ?? 0;
        string adminname = player?.PlayerName ?? Localizer["Console"];

        Task.Run(async () =>
        {
            string playername = await GetPlayerNameFromSteamID(steamid);

            await Database.Unban(steamid);
            await Discord.SendEmbedMessage(playername, steamid, adminname, adminsteamid, string.Empty, -1, false);
        });

        SendMessageToAllPlayers(HudDestination.Chat, "Removed bans matching", adminname, steamid);
    }

    [ConsoleCommand("css_addban")]
    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 2, usage: "<duration> <steamid> [reason]")]
    public void Command_AddBan(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!SteamIDTryParse(args[1], out ulong steamid))
        {
            SendMessageToReplyToCommand(info, "Invalid SteamID specified");
            return;
        }

        if (Database.IsBanned(steamid))
        {
            SendMessageToReplyToCommand(info, "Already banned", steamid);
            return;
        }

        if (player != null && AdminManager.GetPlayerImmunity(player) < AdminManager.GetPlayerAdminData(new SteamID(steamid))?.Immunity)
        {
            SendMessageToReplyToCommand(info, "Unable to target");
            return;
        }

        if (!int.TryParse(args[0], out int duration) || duration < 0)
        {
            duration = 0;
        }

        string reason = string.Join(' ', args[2..]);
        string adminname = player?.PlayerName ?? Localizer["Console"];

        if (duration == 0)
        {
            if (string.IsNullOrEmpty(reason))
            {
                SendMessageToAllPlayers(HudDestination.Chat, "Permabanned", adminname, steamid, duration);
            }
            else
            {
                SendMessageToAllPlayers(HudDestination.Chat, "Permabanned reason", adminname, steamid, duration, reason);
            }
        }
        else
        {
            if (string.IsNullOrEmpty(reason))
            {
                SendMessageToAllPlayers(HudDestination.Chat, "Banned", adminname, steamid, duration);
            }
            else
            {
                SendMessageToAllPlayers(HudDestination.Chat, "Banned reason", adminname, steamid, duration, reason);
            }
        }

        Task.Run(async () =>
        {
            string playername = await GetPlayerNameFromSteamID(steamid);

            await Database.Ban(playername, steamid, adminname, player?.SteamID ?? 0, reason, duration);
            await Discord.SendEmbedMessage(playername, steamid, adminname, player?.SteamID ?? 0, reason, duration, true);
        });

        Utilities.GetPlayerFromSteamId(steamid)?.Disconnect(NetworkDisconnectionReason.NETWORK_DISCONNECT_STEAM_BANNED);
    }
}