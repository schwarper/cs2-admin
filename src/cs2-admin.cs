using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Timers;
using TagsApi;

namespace Admin;

public partial class Admin : BasePlugin, IPluginConfig<AdminConfig>
{
    public override string ModuleName => "Admin";
    public override string ModuleVersion => "0.0.9";
    public override string ModuleAuthor => "schwarper";

    public override void Load(bool hotReload)
    {
        Instance = this;

        Event.Load();

        AddTimer(10.0f, OnBaseCommTimer, TimerFlags.REPEAT);
        AddTimer(60.0f, async () => { await Database.RemoveExpiredBans(); }, TimerFlags.REPEAT);
    }

    public override void OnAllPluginsLoaded(bool hotReload)
    {
        TagApi = ITagApi.Capability.Get();
    }

    public override void Unload(bool hotReload)
    {
        Event.Unload();
    }

    public void OnConfigParsed(AdminConfig config)
    {
        bool usemysql = config.Database.UseMySql;

        if (usemysql)
        {
            if (string.IsNullOrEmpty(config.Database.Host) || string.IsNullOrEmpty(config.Database.Name) || string.IsNullOrEmpty(config.Database.User))
            {
                throw new Exception("You need to setup Database credentials in config.");
            }
        }

        Task.Run(() => Database.CreateDatabaseAsync(config, usemysql));

        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);

        Config = config;
    }

    public static Admin Instance { get; set; } = new();
    public AdminConfig Config { get; set; } = new AdminConfig();
    public static ITagApi? TagApi { get; set; }
}
