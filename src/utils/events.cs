using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using static CounterStrikeSharp.API.Core.Listeners;

namespace Admin;

public partial class Admin : BasePlugin
{
    public void LoadEvents()
    {
        AddCommandListener("say", OnCommandSay);
        AddCommandListener("say_team", OnCommandSay);

        RegisterEventHandler<EventPlayerConnectFull>((@event, info) =>
        {
            CCSPlayerController player = @event.Userid;

            if (player == null || !player.Valid())
            {
                return HookResult.Continue;
            }

            if (IsPlayerPunished(player, "mute"))
            {
                player.VoiceFlags = CounterStrikeSharp.API.VoiceFlags.Muted;
            }

            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerConnect>((@event, info) =>
        {
            CCSPlayerController player = @event.Userid;

            if (player == null || !player.Valid())
            {
                return HookResult.Continue;
            }

            if (IsPlayerPunished(player, "ban"))
            {
                KickPlayer(player, string.Empty);
            }

            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerDeath>((@event, info) =>
        {
            CCSPlayerController player = @event.Userid;

            if (player == null || !player.Valid())
            {
                return HookResult.Continue;
            }

            if (GlobalBeaconTimer.TryGetValue(player, out CounterStrikeSharp.API.Modules.Timers.Timer? timer))
            {
                timer.Kill();
                GlobalBeaconTimer.Remove(player);
            }

            CBasePlayerPawn? playerPawn = player.Pawn.Value;

            if (playerPawn == null || playerPawn.AbsOrigin == null)
            {
                return HookResult.Continue;
            }

            Vector vector = new(playerPawn.AbsOrigin.X, playerPawn.AbsOrigin.Y, playerPawn.AbsOrigin.Z);

            GlobalHRespawnPlayers.Add(player, vector);

            return HookResult.Continue;
        }, HookMode.Post);

        RegisterEventHandler<EventPlayerSpawn>((@event, info) =>
        {
            CCSPlayerController player = @event.Userid;

            if (player == null || !player.Valid())
            {
                return HookResult.Continue;
            }

            if (GlobalBeaconTimer.TryGetValue(player, out CounterStrikeSharp.API.Modules.Timers.Timer? timer))
            {
                timer.Kill();
                GlobalBeaconTimer.Remove(player);
            }

            AddTimer(0.1f, () =>
            {
                CsTeam team = player.Team;

                if (team == CsTeam.CounterTerrorist)
                {
                    player.Health(Config.CTDefaultHealth);
                }
                else
                {
                    player.Health(Config.TDefaultHealth);
                }
            });

            GlobalHRespawnPlayers.Remove(player);

            return HookResult.Continue;
        });

        RegisterListener<OnClientDisconnect>((playerslot) =>
        {
            CCSPlayerController player = Utilities.GetPlayerFromSlot(playerslot);

            if (player == null)
            {
                return;
            }

            if (GlobalBeaconTimer.TryGetValue(player, out CounterStrikeSharp.API.Modules.Timers.Timer? timer))
            {
                timer.Kill();
                GlobalBeaconTimer.Remove(player);
            }
        });
    }

    public HookResult OnCommandSay(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null || !player.Valid() || info.GetArg(1).Length == 0)
        {
            return HookResult.Continue;
        }

        return IsPlayerPunished(player, "gag") ? HookResult.Handled : HookResult.Continue;
    }
}