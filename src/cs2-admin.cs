using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Timers;

namespace Admin;

public partial class Admin : BasePlugin
{
    public override string ModuleName => "Admin";
    public override string ModuleVersion => "0.0.5";
    public override string ModuleAuthor => "schwarper";

    public override void Load(bool hotReload)
    {
        Plugin = this;

        LoadEvents();

        AddTimer(10.0f, () =>
        {
            RemoveExpiredPunishments();
        }, TimerFlags.REPEAT);
    }

    public override void Unload(bool hotReload)
    {
        foreach (Punishment @punishment in GlobalPunishList.Where(p => p is { SaveDatabase: true }))
        {
            if (@punishment.PunishmentName == "ban")
            {
                SaveDatabase(@punishment, "baseban");
            }
            else
            {
                SaveDatabase(@punishment, "basecomm");
            }
        }
    }
}