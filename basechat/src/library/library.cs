using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using static BaseChat.BaseChat;

namespace BaseChat;

public static class Library
{
    public static void SendMessageToAllPlayers(HudDestination destination, string messageKey, params object[] args)
    {
        LocalizedString message = Instance.Localizer[messageKey, args];
        VirtualFunctions.ClientPrintAll(destination, message, 0, 0, 0, 0);
    }

    public static void SendMessageToPlayer(CCSPlayerController player, HudDestination destination, string messageKey, params object[] args)
    {
        LocalizedString message = Instance.Localizer[messageKey, args];
        VirtualFunctions.ClientPrint(player.Handle, destination, message, 0, 0, 0, 0);
    }
}