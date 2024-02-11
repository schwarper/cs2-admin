using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Timers;

namespace Admin;

public partial class Admin : BasePlugin
{
    public override string ModuleName => "Admin";
    public override string ModuleVersion => "0.0.2";
    public override string ModuleAuthor => "schwarper";

    public override void Load(bool hotReload)
    {
        GlobalBasePlugin = this;

        AddCommandListener("say", OnCommandSay);
        AddCommandListener("say_team", OnCommandSay);

        LoadEvents();

        AddTimer(10.0f, () =>
        {
            RemoveExpiredPunishments();
        }, TimerFlags.REPEAT | TimerFlags.STOP_ON_MAPCHANGE);
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
        if (player == null || !player.Valid())
        {
            return HookResult.Continue;
        }

        return IsPlayerPunished(player, "gag") ? HookResult.Stop : HookResult.Continue;
    }
}