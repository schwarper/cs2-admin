using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;

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

            GlobalHRespawnPlayers.Remove(player);

            return HookResult.Continue;
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