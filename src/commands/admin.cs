using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Entities;
using Newtonsoft.Json.Linq;

namespace Admin
{
    public partial class Admin
    {
        private readonly string Filename = Path.Combine(Server.GameDirectory, "csgo", "addons", "counterstrikesharp", "configs", "admins.json");

        [ConsoleCommand("css_addadmin")]
        [RequiresPermissions("@css/ban")]
        [CommandHelper(minArgs: 3, "<steamid> <group> <immunity>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
        public void Command_Addadmin(CCSPlayerController? player, CommandInfo command)
        {
            string steamid = command.GetArg(1);

            if (!SteamID.TryParse(steamid, out SteamID? steamId) || steamId == null)
            {
                command.ReplyToCommand(Config.Tag + Localizer["Must be a steamid"]);
                return;
            }

            steamid = steamId.SteamId64.ToString();

            if (!int.TryParse(command.GetArg(3), out int immunity) || immunity <= 0)
            {
                command.ReplyToCommand(Config.Tag + Localizer["Must be higher than zero"]);
                return;
            }

            string group = command.GetArg(2);

            try
            {
                JObject jsonObject;

                if (File.Exists(Filename))
                {
                    string text = File.ReadAllText(Filename);
                    jsonObject = JObject.Parse(text);
                }
                else
                {
                    jsonObject = new JObject();
                }

                bool adminExists = jsonObject.Properties().Any(item => item.Value["identity"]?.ToString() == steamid);

                if (adminExists)
                {
                    command.ReplyToCommand(Config.Tag + Localizer["css_adminisexist"]);
                    return;
                }

                object newAdmin = new
                {
                    identity = steamId.SteamId64.ToString(),
                    immunity,
                    groups = new List<string> { group }
                };

                jsonObject[steamid] = JToken.FromObject(newAdmin);

                File.WriteAllText(Filename, jsonObject.ToString());

                Server.ExecuteCommand("css_admins_reload");

                command.ReplyToCommand(Config.Tag + Localizer["css_addadmin"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[cs2-admin] Error processing file: " + ex.Message);
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

            if (!File.Exists(Filename))
            {
                command.ReplyToCommand(Config.Tag + Localizer["css_adminnotfound"]);
                return;
            }

            try
            {
                string text = File.ReadAllText(Filename);
                JObject jsonObject = JObject.Parse(text);

                bool adminExists = jsonObject.Properties().Any(item => item.Value["identity"]?.ToString() == steamid);

                if (!adminExists)
                {
                    command.ReplyToCommand(Config.Tag + Localizer["css_adminnotfound"]);
                    return;
                }

                jsonObject.Remove(steamId.SteamId64.ToString());

                File.WriteAllText(Filename, jsonObject.ToString());

                Server.ExecuteCommand("css_admins_reload");

                command.ReplyToCommand(Config.Tag + Localizer["css_removeadmin"]);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[cs2-admin] Error processing file: " + ex.Message);
            }
        }
    }
}