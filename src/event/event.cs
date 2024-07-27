using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using static Admin.Admin;
using static CounterStrikeSharp.API.Core.Listeners;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;

namespace Admin;

public static class Event
{
    public static void Load()
    {
        Instance.AddCommandListener("say", OnSay, HookMode.Pre);
        Instance.AddCommandListener("say_team", OnSay, HookMode.Pre);

        Instance.RegisterListener<OnClientAuthorized>(OnClientAuthorized);

        Instance.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        Instance.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        Instance.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);
        Instance.RegisterEventHandler<EventRoundStart>(OnRoundStart);
        Instance.RegisterEventHandler<EventPlayerConnectFull>(OnPlayerConnect);
    }

    public static void Unload()
    {
        Instance.RemoveCommandListener("say", OnSay, HookMode.Pre);
        Instance.RemoveCommandListener("say_team", OnSay, HookMode.Pre);
        Instance.RemoveListener<OnClientAuthorized>(OnClientAuthorized);
    }

    public static HookResult OnSay(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            return HookResult.Continue;
        }

        if (PlayerGagList.Contains(player.SteamID))
        {
            player.PrintToChat(Instance.Config.Tag + Instance.Localizer["You are gagged"]);
            return HookResult.Handled;
        }

        PunishInfo? punish = PlayerTemporaryPunishList.FirstOrDefault(p => p.SteamID == player.SteamID && p.PunishName == "GAG");

        if (punish != null)
        {
            player.PrintToChat(Instance.Config.Tag + Instance.Localizer["You are gagged temp", punish.Duration, punish.Created, punish.End]);
            return HookResult.Handled;
        }

        return HookResult.Continue;
    }

    public static HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        DeleteBeaconTimer(player);

        GlobalHRespawnPlayers.Remove(player);

        player.PlayerPawn.Value?.Glow(Color.White);

        Instance.AddTimer(0.1f, () =>
        {
            CsTeam team = player.Team;

            if (team == CsTeam.CounterTerrorist)
            {
                player.Health(Instance.Config.CTDefaultHealth);
            }
            else if (team == CsTeam.Terrorist)
            {
                player.Health(Instance.Config.TDefaultHealth);
            }
        });

        return HookResult.Continue;
    }
    public static HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        DeleteBeaconTimer(player);

        Vector? absOrigin = player.PlayerPawn.Value?.AbsOrigin;

        if (absOrigin == null)
        {
            return HookResult.Continue;
        }

        if (GlobalHRespawnPlayers.TryGetValue(player, out (float X, float Y, float Z) value))
        {
            value.X = absOrigin.X;
            value.Y = absOrigin.Y;
            value.Z = absOrigin.Z;
        }
        else
        {
            GlobalHRespawnPlayers.Add(player, (absOrigin.X, absOrigin.Y, absOrigin.Z));
        }

        return HookResult.Continue;
    }

    public static async void OnClientAuthorized(int slot, SteamID steamId)
    {
        CCSPlayerController? player = Utilities.GetPlayerFromSlot(slot);

        if (player == null || player.IsBot)
        {
            return;
        }

        int? userid = player.UserId;

        if (await Database.IsBanned(steamId.SteamId64))
        {
            Server.NextFrame(() => Server.ExecuteCommand($"kickid {userid}"));
        }
    }

    public static HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        DeleteBeaconTimer(player);

        GlobalHRespawnPlayers.Remove(player);
        PlayerGagList.Remove(player.SteamID);
        TagApi?.UngagPlayer(player.SteamID);
        PlayerTemporaryPunishList.RemoveAll(p => p.SteamID == player.SteamID);

        return HookResult.Continue;
    }

    public static HookResult OnRoundStart(EventRoundStart @event, GameEventInfo info)
    {
        PlayerGagList.Clear();
        return HookResult.Continue;
    }

    public static HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        Task.Run(() => Database.LoadPlayer(player));
        return HookResult.Continue;
    }

    private static void DeleteBeaconTimer(CCSPlayerController player)
    {
        if (GlobalBeaconTimer.TryGetValue(player, out Timer? timer) && timer != null)
        {
            timer.Kill();
            GlobalBeaconTimer.Remove(player);
        }
    }
}