using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
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
            info.ReplyToCommand(Instance.Config.Tag + Instance.Localizer.ForPlayer(info.CallingPlayer, messageKey, args));
        }
    }

    public static string NormalizeGroup(string group)
    {
        return group[0] != '#' ? '#' + group : group;
    }
}