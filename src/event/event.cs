using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Entities;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using static Admin.Admin;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;
using Vector = CounterStrikeSharp.API.Modules.Utils.Vector;
using static CounterStrikeSharp.API.Core.Listeners;

namespace Admin;

public static class Event
{
    public static void Load()
    {
        Instance.RegisterEventHandler<EventPlayerSpawn>(OnPlayerSpawn);
        Instance.RegisterEventHandler<EventPlayerDeath>(OnPlayerDeath);
        Instance.RegisterEventHandler<EventPlayerDisconnect>(OnPlayerDisconnect);

        Instance.RegisterListener<OnClientAuthorized>(OnClientAuthorized);
    }

    public static void Unload()
    {
        Instance.RemoveListener<Listeners.OnClientAuthorized>(OnClientAuthorized);
    }

    public static HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        DeleteBeaconTimer(player);

        Instance.AddTimer(0.1f, () =>
        {
            CsTeam team = player.Team;

            if (team == CsTeam.CounterTerrorist)
            {
                player.Health(Instance.Config.CTDefaultHealth);
            }
            else
            {
                player.Health(Instance.Config.TDefaultHealth);
            }

            player.PlayerPawn.Value?.Glow(Color.White);
        });

        GlobalHRespawnPlayers.Remove(player);

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

        GlobalHRespawnPlayers.Add(player, absOrigin);

        return HookResult.Continue;
    }

    public static async void OnClientAuthorized(int slot, SteamID steamId)
    {
        if (await Database.IsBanned(steamId.SteamId64))
        {
            return;
        }

        int? userid = Utilities.GetPlayerFromSlot(slot)?.UserId;

        if (userid == null)
        {
            return;
        }

        Server.NextFrame(() => Server.ExecuteCommand($"kickid {userid}"));
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