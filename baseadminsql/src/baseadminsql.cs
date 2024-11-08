using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using static BaseAdminSql.Library;

namespace BaseAdminSql;

public class BaseAdminSql : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Base Admin SQL";
    public override string ModuleVersion => "1.7";
    public override string ModuleAuthor => "schwarper";
    public override string ModuleDescription => "(SQL) Basic Admin Manager Plugin";

    public static BaseAdminSql Instance { get; set; } = new();
    public Config Config { get; set; } = new Config();

    public override void Load(bool hotReload)
    {
        Instance = this;

        AddCommandListener("css_admins_reload", OnAdminsReload, HookMode.Post);
        AddCommandListener("css_groups_reload", OnGroupsReload, HookMode.Post);
    }

    public override void Unload(bool hotReload)
    {
        RemoveCommandListener("css_admins_reload", OnAdminsReload, HookMode.Post);
        RemoveCommandListener("css_groups_reload", OnGroupsReload, HookMode.Post);
    }

    public async void OnConfigParsed(Config config)
    {
        await Database.CreateDatabaseAsync(config);
        await Database.LoadGroups();
        await Database.LoadAdmins();

        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);
        Config = config;
    }

    public static HookResult OnAdminsReload(CCSPlayerController? player, CommandInfo info)
    {
        Task.Run(Database.LoadAdmins);
        return HookResult.Continue;
    }

    public static HookResult OnGroupsReload(CCSPlayerController? player, CommandInfo info)
    {
        Task.Run(Database.LoadGroups);
        return HookResult.Continue;
    }

    [ConsoleCommand("css_addadmin")]
    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 2, "<steamid> <group> <immunity>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Addadmin(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string steamIdText = args[0];

        if (!SteamIDTryParse(steamIdText, out ulong steamId))
        {
            SendMessageToReplyToCommand(info, true, "Invalid SteamID specified");
            return;
        }

        if (AdminManagerEx.IsAdminExist(steamId))
        {
            SendMessageToReplyToCommand(info, true, "Admin already exists");
            return;
        }

        uint immunity = args.Length >= 3 && uint.TryParse(args[2], out uint parsedImmunity) ? parsedImmunity : 0;

        HashSet<string> groups = [NormalizeGroup(args[1])];

        Task.Run(() => Database.AddAdmin(steamId, [], groups, immunity));
        SendMessageToReplyToCommand(info, true, "Admin has been added");
    }

    [ConsoleCommand("css_removeadmin")]
    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 1, "<steamid>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Removeadmin(CCSPlayerController? player, CommandInfo info)
    {
        if (!SteamIDTryParse(info.GetArg(1), out ulong steamId))
        {
            SendMessageToReplyToCommand(info, true, "Invalid SteamID specified");
            return;
        }

        if (!AdminManagerEx.IsAdminExist(steamId))
        {
            SendMessageToReplyToCommand(info, true, "Admin does not exist");
            return;
        }

        Task.Run(() => Database.RemoveAdmin(steamId));
        SendMessageToReplyToCommand(info, true, "Admin has been removed");
    }

    [ConsoleCommand("css_addgroup")]
    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 2, "<group> <flags> <immunity>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Addgroup(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string group = NormalizeGroup(args[0]);

        if (AdminManagerEx.IsGroupExist(group))
        {
            SendMessageToReplyToCommand(info, true, "Admin group already exists");
            return;
        }

        uint immunity = args.Length >= 3 && uint.TryParse(args[2], out uint parsedImmunity) ? parsedImmunity : 0;

        HashSet<string> flags = args[1]
         .Split(',', StringSplitOptions.RemoveEmptyEntries)
         .Select(flag => flag.Trim().StartsWith("@css/") ? flag.Trim() : "@css/" + flag.Trim())
         .ToHashSet();

        Task.Run(() => Database.AddGroup(group, flags, immunity));
        SendMessageToReplyToCommand(info, true, "Admin group has been added");
    }

    [ConsoleCommand("css_removegroup")]
    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 1, "<group>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Removegroup(CCSPlayerController? player, CommandInfo info)
    {
        string group = NormalizeGroup(info.GetArg(1));

        if (!AdminManagerEx.IsGroupExist(group))
        {
            SendMessageToReplyToCommand(info, true, "Admin group does not exist");
            return;
        }

        Task.Run(() => Database.RemoveGroup(group));
        SendMessageToReplyToCommand(info, true, "Admin group has been removed");
    }
}