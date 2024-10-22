using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Newtonsoft.Json.Linq;
using static BaseAdmin.Library;

namespace BaseAdmin;

public class BaseAdmin : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Base Admin";
    public override string ModuleVersion => "1.4";
    public override string ModuleAuthor => "schwarper";
    public override string ModuleDescription => "Allows to add & remove admin";

    public readonly string AdminFile = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "admins.json");
    public readonly string AdminGroupsFile = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "admin_groups.json");
    public static BaseAdmin Instance { get; set; } = new();
    public Config Config { get; set; } = new Config();

    public override void Load(bool hotReload)
    {
        Instance = this;
    }

    public void OnConfigParsed(Config config)
    {
        config.Tag = config.Tag.ReplaceColorTags();
        Config = config;
    }

    [ConsoleCommand("css_addadmin")]
    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 2, "<steamid> <group> <immunity>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Addadmin(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string steamIdText = args[0];

        if (!SteamIDTryParse(steamIdText, out _))
        {
            SendMessageToReplyToCommand(info, true, "Invalid SteamID specified");
            return;
        }

        if (ReadText(AdminFile, out JObject jsonObject))
        {
            if (jsonObject[steamIdText] != null)
            {
                SendMessageToReplyToCommand(info, true, "Admin already exists");
                return;
            }
        }

        int immunity = args.Length >= 3 && int.TryParse(args[2], out var parsedImmunity) ? parsedImmunity : 0;
        string group = NormalizeGroup(args[1]);

        var newItem = new
        {
            identity = steamIdText,
            immunity,
            groups = new[] { group }
        };

        jsonObject[steamIdText] = JToken.FromObject(newItem);
        WriteJsonToFile(AdminFile, jsonObject);
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

        string steamIdText = steamId.ToString();

        if (!ReadText(AdminFile, out JObject jsonObject) || jsonObject[steamIdText] == null)
        {
            SendMessageToReplyToCommand(info, true, "Admin does not exist");
            return;
        }

        jsonObject.Remove(steamIdText);
        WriteJsonToFile(AdminFile, jsonObject);
        SendMessageToReplyToCommand(info, true, "Admin has been removed");
    }

    [ConsoleCommand("css_addgroup")]
    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 2, "<group> <flags> <immunity>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Addgroup(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        string group = NormalizeGroup(args[0]);

        if (ReadText(AdminGroupsFile, out JObject jsonObject))
        {
            if (jsonObject[group] != null)
            {
                SendMessageToReplyToCommand(info, true, "Admin group already exists");
                return;
            }
        }

        int immunity = args.Length >= 3 && int.TryParse(args[2], out var parsedImmunity) ? parsedImmunity : 0;

        var flags = args[1]
         .Split(',', StringSplitOptions.RemoveEmptyEntries)
         .Select(flag => flag.Trim().StartsWith("@css/") ? flag.Trim() : "@css/" + flag.Trim())
         .ToList();


        var newItem = new
        {
            flags,
            immunity
        };

        jsonObject[group] = JToken.FromObject(newItem);
        WriteJsonToFile(AdminGroupsFile, jsonObject);
        SendMessageToReplyToCommand(info, true, "Admin group has been added");
    }

    [ConsoleCommand("css_removegroup")]
    [RequiresPermissions("@css/root")]
    [CommandHelper(minArgs: 1, "<group>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Removegroup(CCSPlayerController? player, CommandInfo info)
    {
        var group = NormalizeGroup(info.GetArg(1));

        if (ReadText(AdminGroupsFile, out JObject jsonObject))
        {
            if (jsonObject[group] == null)
            {
                SendMessageToReplyToCommand(info, true, "Admin group does not exist");
                return;
            }
        }

        jsonObject.Remove(group);
        WriteJsonToFile(AdminGroupsFile, jsonObject);
        SendMessageToReplyToCommand(info, true, "Admin group has been removed");
    }
}