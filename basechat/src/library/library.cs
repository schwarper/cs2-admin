using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using static BaseChat.BaseChat;

namespace BaseChat;

public static class Library
{
    public const string playerdesignername = "cs_player_controller";

    public static void SendMessageToAdmins(string playername, string message)
    {
        for (int i = 0; i < Server.MaxPlayers; i++)
        {
            CCSPlayerController? target = Utilities.GetEntityFromIndex<CCSPlayerController>(i + 1);

            if (target?.DesignerName != playerdesignername)
            {
                continue;
            }

            if (!AdminManager.PlayerHasPermissions(target.AuthorizedSteamID, "@css/chat"))
            {
                continue;
            }

            SendMessageToPlayer(target, HudDestination.Chat, "css_asay", playername, message);
        }
    }

    public static void SendMessageToPlayer(CCSPlayerController player, HudDestination destination, string messageKey, params object[] args)
    {
        using (new WithTemporaryCulture(player.GetLanguage()))
        {
            LocalizedString message = Instance.Localizer[messageKey, args];
            VirtualFunctions.ClientPrint(player.Handle, destination, message, 0, 0, 0, 0);
        }
    }

    public static void SendMessageToAllPlayers(HudDestination destination, string messageKey, params object[] args)
    {
        for (int i = 0; i < Server.MaxPlayers; i++)
        {
            CCSPlayerController? player = Utilities.GetEntityFromIndex<CCSPlayerController>(i + 1);

            if (player?.IsValid is not true || player.IsBot || player.DesignerName != playerdesignername || player.Connected != PlayerConnectedState.PlayerConnected)
            {
                continue;
            }

            SendMessageToPlayer(player, destination, messageKey, args);
        }
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
}