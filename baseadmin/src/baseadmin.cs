using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using static BaseAdmin.Library;

namespace BaseAdmin;

public class BaseAdmin : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Base Admin";
    public override string ModuleVersion => "1.3";
    public override string ModuleAuthor => "schwarper";
    public override string ModuleDescription => "Allows to add & remove admin";

    public readonly string AdminFile = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "admins.json");
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
    [RequiresPermissions("@css/ban")]
    [CommandHelper(minArgs: 2, "<steamid> <group> <immunity>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Addadmin(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!SteamIDTryParse(args[0], out ulong steamId))
        {
            SendMessageToReplyToCommand(info, true, "Invalid SteamID specified");
            return;
        }

        Console.WriteLine(args.Length);

        if (args.Length < 3 || !int.TryParse(args[2], out var immunity))
        {
            immunity = 0;
        }

        try
        {
            dynamic newItem = new
            {
                identity = $"{steamId}",
                immunity,
                groups = new[] { $"#{args[1]}" }
            };

            string updatedJson;

            if (File.Exists(AdminFile))
            {
                string text = File.ReadAllText(AdminFile);

                JObject jsonObject = JObject.Parse(text);

                if (jsonObject[$"[{steamId}]"] != null)
                {
                    SendMessageToReplyToCommand(info, true, "Admin already exists");
                    return;
                }

                jsonObject[$"[{steamId}]"] = JToken.FromObject(newItem);

                updatedJson = jsonObject.ToString();
            }
            else
            {
                JObject jsonObject = new()
                {
                    [steamId] = JToken.FromObject(newItem)
                };

                updatedJson = jsonObject.ToString();
            }

            File.WriteAllText(AdminFile, updatedJson);

            Server.ExecuteCommand("css_admins_reload");

            SendMessageToReplyToCommand(info, true, "Admin has been added");
        }
        catch (Exception ex)
        {
            Console.WriteLine("[cs2-admin] Error reading file: " + ex.Message);
        }
    }

    [ConsoleCommand("css_removeadmin")]
    [RequiresPermissions("@css/ban")]
    [CommandHelper(minArgs: 1, "<steamid>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Removeadmin(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!SteamIDTryParse(args[0], out ulong steamId))
        {
            SendMessageToReplyToCommand(info, true, "Invalid SteamID specified");
            return;
        }

        if (File.Exists(AdminFile))
        {
            try
            {
                string text = File.ReadAllText(AdminFile);

                JObject jsonObject = JObject.Parse(text);

                if (jsonObject[$"[{steamId}]"] != null)
                {
                    jsonObject.Remove($"[{steamId}]");
                    string updatedJson = jsonObject.ToString();
                    File.WriteAllText(AdminFile, updatedJson);

                    Server.ExecuteCommand("css_admins_reload");
                    SendMessageToReplyToCommand(info, true, "Admin has been removed");
                }
                else
                {
                    SendMessageToReplyToCommand(info, true, "Admin does not exist");
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine("[cs2-admin] Error reading or writing file: " + ex.Message);
            }
        }
    }
}