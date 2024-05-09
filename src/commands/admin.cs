using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using Newtonsoft.Json.Linq;

namespace Admin;

public partial class Admin
{
    public readonly string GlobalAdminsFilename = $"{Server.GameDirectory}/csgo/addons/counterstrikesharp/configs/admins.json";

    [ConsoleCommand("css_addadmin")]
    [RequiresPermissions("@css/ban")]
    [CommandHelper(minArgs: 2, "<steamid> <group> <immunity>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Addadmin(CCSPlayerController? player, CommandInfo command)
    {
        string steamid = command.GetArg(1);

        if (!SteamID.TryParse(steamid, out SteamID? steamId) || steamId == null)
        {
            command.ReplyToCommand(Config.Tag + Localizer["Must be a steamid"]);
            return;
        }

        string group = command.GetArg(2);

        if (!int.TryParse(command.GetArg(3), out int immunity))
        {
            immunity = 0;
        }

        try
        {
            dynamic newItem = new
            {
                groups = new[] { $"#{group}" },
                identity = $"{steamId.SteamId64}",
                immunity,
            };

            string updatedJson;

            if (File.Exists(GlobalAdminsFilename))
            {
                string text = File.ReadAllText(GlobalAdminsFilename);

                JObject jsonObject = JObject.Parse(text);

                if (jsonObject[steamid] != null)
                {
                    command.ReplyToCommand(Config.Tag + Localizer["css_adminisexist"]);
                    return;
                }

                jsonObject[steamid] = JToken.FromObject(newItem);

                updatedJson = jsonObject.ToString();
            }
            else
            {
                JObject jsonObject = new()
                {
                    [steamid] = JToken.FromObject(newItem)
                };

                updatedJson = jsonObject.ToString();
            }

            File.WriteAllText(GlobalAdminsFilename, updatedJson);

            Server.ExecuteCommand("css_admins_reload");

            command.ReplyToCommand(Config.Tag + Localizer["css_addadmin"]);
        }
        catch (Exception ex)
        {
            Console.WriteLine("[cs2-admin] Error reading file: " + ex.Message);
        }
    }

    [ConsoleCommand("css_removeadmin")]
    [RequiresPermissions("@css/ban")]
    [CommandHelper(minArgs: 1, "<steamid>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Removeadmin(CCSPlayerController? player, CommandInfo command)
    {
        string steamid = command.GetArg(1);

        if (!SteamID.TryParse(steamid, out SteamID? steamId) || steamId == null)
        {
            command.ReplyToCommand(Config.Tag + Localizer["Must be a steamid"]);
            return;
        }

        if (File.Exists(GlobalAdminsFilename))
        {
            try
            {
                string text = File.ReadAllText(GlobalAdminsFilename);

                JObject jsonObject = JObject.Parse(text);

                if (jsonObject[steamid] != null)
                {
                    jsonObject.Remove(steamid);

                    string updatedJson = jsonObject.ToString();

                    File.WriteAllText(GlobalAdminsFilename, updatedJson);

                    Server.ExecuteCommand("css_admins_reload");

                    command.ReplyToCommand(Config.Tag + Localizer["css_removeadmin"]);
                }
                else
                {
                    command.ReplyToCommand(Config.Tag + Localizer["css_adminnotfound"]);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("[cs2-admin] Error reading or writing file: " + ex.Message);
            }
        }
    }
}