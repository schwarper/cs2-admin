using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using static BaseVotes.BaseVotes;

namespace BaseVotes;

public static class Library
{
    public const string playerdesignername = "cs_player_controller";

    public static void SendMessageToPlayer(CCSPlayerController player, HudDestination destination, string messageKey, params object[] args)
    {
        player.PrintToChat(Instance.Config.Tag + Instance.Localizer.ForPlayer(player, messageKey, args));
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

    public static void SendMessageToReplyToCommand(CommandInfo info, string messageKey, params object[] args)
    {
        if (info.CallingPlayer == null)
        {
            Server.PrintToConsole(Instance.Config.Tag + Instance.Localizer[messageKey, args]);
        }
        else
        {
            SendMessageToPlayer(info.CallingPlayer,
                info.CallingContext == CommandCallingContext.Console ? HudDestination.Console : HudDestination.Chat,
                messageKey,
                args);
        }
    }
}