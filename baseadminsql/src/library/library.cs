using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using static BaseAdminSql.BaseAdminSql;

namespace BaseAdminSql;

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
        CounterStrikeSharp.API.Core.CCSPlayerController? player = info.CallingPlayer;

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

    public static string NormalizeGroup(string group)
    {
        return group[0] != '#' ? '#' + group : group;
    }
}