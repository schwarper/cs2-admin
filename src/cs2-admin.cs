using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Timers;

namespace Admin;

public partial class Admin : BasePlugin, IPluginConfig<AdminConfig>
{
    public override string ModuleName => "Admin";
    public override string ModuleVersion => "0.0.8";
    public override string ModuleAuthor => "schwarper";

    public override void Load(bool hotReload)
    {
        Instance = this;

        Event.Load();

        AddTimer(10.0f, OnBaseCommTimer, TimerFlags.REPEAT);
        AddTimer(60.0f, async () => { await Database.RemoveExpiredBans(); }, TimerFlags.REPEAT);
    }

    public override void Unload(bool hotReload)
    {
        Event.Unload();
    }

    public void OnConfigParsed(AdminConfig config)
    {
        string[] databaseStrings = ["host", "name", "user"];

        if (databaseStrings.Any(p => string.IsNullOrEmpty(config.Database[p])))
        {
            throw new Exception("You need to setup Database credentials in config.");
        }

        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);

        Task.Run(() => Database.CreateDatabaseAsync(config));

        Config = config;
    }

    public static Admin Instance { get; set; } = new();
    public AdminConfig Config { get; set; } = new AdminConfig();
}

/*
 * FUNCOMMANDS (HRESPAWN, STRIP?!, 
 * BASEVOTE (TÜRKÇE KARAKTER YOK?, TABLO GÜNCELLENMÝYOR, TIMER DURMUYOR)
 * 
*/
