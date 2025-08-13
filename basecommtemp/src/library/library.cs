using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Utils;
using static BaseCommTemp.BaseCommTemp;

namespace BaseCommTemp;

public static class Library
{
    public const string playerdesignername = "cs_player_controller";

    public static bool ProcessTargetString(
        CCSPlayerController? player,
        CommandInfo info,
        string targetstr,
        out List<CCSPlayerController> players,
        out string adminname,
        out string targetname)
    {
        players = [];
        adminname = string.Empty;
        targetname = string.Empty;

        TargetResult targetResult = new Target(targetstr).GetTarget(player);

        if (targetResult.Players.Count == 0)
        {
            SendMessageToReplyToCommand(info, "No matching client");
            return false;
        }
        else if (targetResult.Players.Count > 1)
        {
            SendMessageToReplyToCommand(info, "More than one client matched");
            return false;
        }

        CCSPlayerController target = targetResult.Players[0];

        if (!AdminManager.CanPlayerTarget(player, target))
        {
            SendMessageToReplyToCommand(info, "Unable to target");
            return false;
        }

        targetname = targetResult.Players[0].PlayerName;
        adminname = player?.PlayerName ?? Instance.Localizer["Console"];
        players = [player];
        return true;
    }

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