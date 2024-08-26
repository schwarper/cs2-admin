using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Capabilities;
using CounterStrikeSharp.API.Core.Translations;

namespace Admin;

public partial class Admin : BasePlugin, IPluginConfig<AdminConfig>
{
    public override string ModuleName => "Admin";
    public override string ModuleVersion => "1.1";
    public override string ModuleAuthor => "schwarper";

    public override void Load(bool hotReload)
    {
        Instance = this;

        Event.Load();
    }

    public override void Unload(bool hotReload)
    {
        Event.Unload();
    }

    public void OnConfigParsed(AdminConfig config)
    {
        if (config.Database.UseMySql)
        {
            if (string.IsNullOrEmpty(config.Database.Host) || string.IsNullOrEmpty(config.Database.Name) || string.IsNullOrEmpty(config.Database.User))
            {
                throw new Exception("You need to setup Database credentials in config.");
            }
        }
        else
        {
            string filename = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "cs2-admin.db");
            Database.SetFileName(filename);
        }

        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);

        Task.Run(() => Database.CreateDatabaseAsync(config));

        Config = config;
    }

    public static Admin Instance { get; set; } = new();
    public AdminConfig Config { get; set; } = new AdminConfig();
}
