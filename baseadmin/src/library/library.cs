using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using Newtonsoft.Json.Linq;
using static BaseAdmin.BaseAdmin;

namespace BaseAdmin;

public static class Library
{
    public static bool SteamIDTryParse(string id, out ulong steamId)
    {
        steamId = 0;

        if (id.Length != 17)
        {
            return false;
        }

        if (!ulong.TryParse(id, out steamId))
        {
            return false;
        }

        const ulong minSteamID = 76561197960265728;
        return steamId >= minSteamID;
    }

    public static void SendMessageToReplyToCommand(CommandInfo info, bool addTag, string messageKey, params object[] args)
    {
        CCSPlayerController? player = info.CallingPlayer;

        if (player == null)
        {
            Server.PrintToConsole(Instance.Config.Tag + Instance.Localizer[messageKey, args]);
        }
        else
        {
            HudDestination destination = info.CallingContext == CommandCallingContext.Console ? HudDestination.Console : HudDestination.Chat;

            using (new WithTemporaryCulture(player.GetLanguage()))
            {
                LocalizedString message = Instance.Localizer[messageKey, args];
                VirtualFunctions.ClientPrint(player.Handle, destination, (addTag == true ? Instance.Config.Tag : string.Empty) + message, 0, 0, 0, 0);
            }
        }
    }

    public static bool ReadText(string filename, out JObject jsonObject)
    {
        jsonObject = [];

        if (File.Exists(filename))
        {
            try
            {
                string text = File.ReadAllText(filename);
                jsonObject = JObject.Parse(text);
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[cs2-admin] Error parsing file: {ex.Message}");
                return false;
            }
        }

        return false;
    }

    public static string NormalizeGroup(string group)
    {
        return group[0] != '#' ? '#' + group : group;
    }

    public static void WriteJsonToFile(string filePath, JObject jsonObject)
    {
        File.WriteAllText(filePath, jsonObject.ToString());
        Server.ExecuteCommand(filePath.Contains("admins") ? "css_admins_reload" : "css_groups_reload");
    }
}