using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Commands;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using Source2Framework.Models;
using System.Reflection;
using System.Text;

namespace AdminHelp;

public class AdminHelp : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Admin Help";
    public override string ModuleVersion => "1.7";
    public override string ModuleAuthor => "schwarper & KillStr3aK";
    public override string ModuleDescription => "Display command information";

    private const int COMMANDS_PER_PAGE = 10;
    public Config Config { get; set; } = new Config();

    public HashSet<string> IgnoreCommands = ["css", "css_1", "css_2", "css_3", "css_4", "css_5", "css_6", "css_7", "css_8", "css_9"];

    public void OnConfigParsed(Config config)
    {
        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);
        Config = config;
    }

    [ConsoleCommand("css_help")]
    [ConsoleCommand("css_searchcmd")]
    public void Command_Help(CCSPlayerController? player, CommandInfo info)
    {
        CommandManager? commandManager = Reflection.GetFieldValue<CommandManager, PluginCommandManagerDecorator>(CommandManager as PluginCommandManagerDecorator, "_inner");

        if (commandManager == null)
        {
            info.ReplyToCommand(GetFormattedMessage(player, "Unable to get command manager"));
            return;
        }

        Dictionary<string, IList<CommandDefinition>>? commandDefinitions = Reflection.GetFieldValue<Dictionary<string, IList<CommandDefinition>>, CommandManager>(commandManager, "_commandDefinitions");

        if (commandDefinitions == null)
        {
            info.ReplyToCommand(GetFormattedMessage(player, "Unable to get command definitions"));
            return;
        }

        if (info.CallingContext == CommandCallingContext.Chat)
        {
            info.ReplyToCommand(GetFormattedMessage(player, "See console for output"));
        }

        string arg = info.GetArg(1);

        bool doSearch = !info.GetArg(0).Equals("css_help", StringComparison.CurrentCultureIgnoreCase);
        int pageNum = Math.Max(1, int.TryParse(arg, out int parsedPageNum) ? parsedPageNum : 1);
        int startCmd = (pageNum - 1) * COMMANDS_PER_PAGE;
        string noDescriptionText = Localizer.ForPlayer(player, "No description available");

        var commandsToShow = commandDefinitions
            .Where(commandPair =>
            {
                if (IgnoreCommands.Contains(commandPair.Key))
                {
                    return false;
                }

                if (doSearch && !commandPair.Key.Contains(arg, StringComparison.CurrentCultureIgnoreCase))
                {
                    return false;
                }

                CommandDefinition? firstCommandDef = commandPair.Value.FirstOrDefault();
                return CanUse(player, commandPair.Key, firstCommandDef?.Callback);
            })
            .Skip(startCmd)
            .Select((commandPair, index) =>
            {
                CommandDefinition? firstCommandDef = commandPair.Value.FirstOrDefault();

                return new
                {
                    Index = startCmd + index + 1,
                    Name = commandPair.Key,
                    Description = firstCommandDef?.Description ?? noDescriptionText,
                    UsageHint = firstCommandDef?.UsageHint ?? string.Empty
                };
            }).ToList();

        if (commandsToShow.Count == 0)
        {
            ReplyMessage(player, GetFormattedMessage(player, "No commands available or no matching results found"));
        }
        else
        {
            var commands = doSearch ? commandsToShow : commandsToShow.Take(COMMANDS_PER_PAGE);

            foreach (var command in commands)
            {
                StringBuilder sb = new StringBuilder()
                    .Append($"[{command.Index}] ")
                    .Append(command.Name);

                if (!string.IsNullOrWhiteSpace(command.UsageHint))
                {
                    sb.Append(' ').Append(command.UsageHint);
                }

                sb.Append(" - ").Append(command.Description);

                ReplyMessage(player, sb.ToString());
            }

            if (!doSearch && commandsToShow.Count > COMMANDS_PER_PAGE)
            {
                ReplyMessage(player, GetFormattedMessage(player, "Type css_help to see more commands", pageNum + 1));
            }
        }
    }

    private static bool CanUse(CCSPlayerController? player, string name, CommandInfo.CommandCallback? callback)
    {
        if (player == null)
        {
            return true;
        }

        List<BaseRequiresPermissions> permissionsToCheck = [];

        if (AdminManager.CommandIsOverriden(name))
        {
            CommandData? data = AdminManager.GetCommandOverrideData(name);

            if (data != null)
            {
                Type attrType = (data.CheckType == "all") ? typeof(RequiresPermissions) : typeof(RequiresPermissionsOr);
                BaseRequiresPermissions? attr = (BaseRequiresPermissions?)Activator.CreateInstance(attrType, args: AdminManager.GetPermissionOverrides(name));

                if (attr != null) permissionsToCheck.Add(attr);
            }
        }
        else
        {
            IEnumerable<BaseRequiresPermissions>? permissions = callback?.Method.GetCustomAttributes<BaseRequiresPermissions>();
            if (permissions != null) permissionsToCheck.AddRange(permissions);
        }

        if (permissionsToCheck.Count == 0)
        {
            return true;
        }

        foreach (BaseRequiresPermissions attr in permissionsToCheck)
        {
            if (attr.Permissions.Count == 0)
            {
                continue;
            }

            attr.Command = name;

            if (!attr.CanExecuteCommand(player))
            {
                return false;
            }
        }

        return true;
    }

    private string GetFormattedMessage(CCSPlayerController? player, string message, params object[] args)
    {
        return Config.Tag + Localizer.ForPlayer(player, message, args);
    }

    private static void ReplyMessage(CCSPlayerController? player, string message)
    {
        if (player == null)
        {
            Server.PrintToConsole(message);
        }
        else
        {
            player.PrintToConsole(message);
        }
    }
}