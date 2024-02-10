using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;

namespace Admin;

public partial class Admin : BasePlugin
{
    public override string ModuleName => "Admin";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "schwarper";

    public override void Load(bool hotReload)
    {
        GlobalBasePlugin = this;

        AddCommandListener("say", OnCommandSay);
        AddCommandListener("say_team", OnCommandSay);

        RegisterEventHandler<EventPlayerConnectFull>((@event, info) =>
        {
            var player = @event.Userid;

            if (player == null || !player.IsValid)
            {
                return HookResult.Continue;
            }

            if(IsPlayerPunished(player, "mute"))
            {
                player.VoiceFlags = CounterStrikeSharp.API.VoiceFlags.Muted;
            }

            return HookResult.Continue;
        });

        RegisterEventHandler<EventPlayerConnect>((@event, info) =>
        {
            var player = @event.Userid;

            if (player == null || !player.IsValid)
            {
                return HookResult.Continue;
            }

            if(IsPlayerPunished(player, "ban"))
            {
                KickPlayer(player, string.Empty);
            }

            return HookResult.Continue;
        });

        AddTimer(10.0f, () =>
        {
            RemoveExpiredPunishments();
        }, CounterStrikeSharp.API.Modules.Timers.TimerFlags.REPEAT | CounterStrikeSharp.API.Modules.Timers.TimerFlags.STOP_ON_MAPCHANGE);
    }

    public override void Unload(bool hotReload)
    {
        foreach (Punishment @punishment in GlobalPunishList.Where(p => p is { SaveDatabase: true }))
        {
            if(@punishment.PunishmentName == "ban")
            {
                SaveDatabase(@punishment, "baseban");
            }
            else
            {
                SaveDatabase(@punishment, "basecomm");
            }
        }
    }

    public HookResult OnCommandSay(CCSPlayerController? player, CommandInfo info)
    {
        if(player == null || !player.IsValid)
        {
            return HookResult.Continue;
        }

        return IsPlayerPunished(player, "gag") ? HookResult.Handled : HookResult.Continue;
    }
}