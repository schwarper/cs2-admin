using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using static BaseComm.BaseComm;
using static CounterStrikeSharp.API.Modules.Commands.Targeting.Target;

namespace BaseComm;

public static class Library
{
    public static bool ProcessTargetString(
        CCSPlayerController? player,
        CommandInfo info,
        string targetstr,
        bool singletarget,
        bool immunitycheck,
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
            info.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["No matching client"]);
            return false;
        }
        else if (targetResult.Players.Count > 1)
        {
            if (singletarget || !TargetTypeMap.ContainsKey(targetstr))
            {
                info.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["More than one client matched"]);
                return false;
            }
        }

        if (immunitycheck)
        {
            targetResult.Players.RemoveAll(target => !AdminManager.CanPlayerTarget(player, target));

            if (targetResult.Players.Count == 0)
            {
                info.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["You cannot target"]);
                return false;
            }
        }

        TargetTypeMap.TryGetValue(targetstr, out TargetType type);

        targetname = type switch
        {
            TargetType.GroupAll => Instance.Localizer["all"],
            TargetType.GroupBots => Instance.Localizer["bots"],
            TargetType.GroupHumans => Instance.Localizer["humans"],
            TargetType.GroupAlive => Instance.Localizer["alive"],
            TargetType.GroupDead => Instance.Localizer["dead"],
            TargetType.GroupNotMe => Instance.Localizer["notme"],
            TargetType.PlayerMe => targetResult.Players.First().PlayerName,
            TargetType.TeamCt => Instance.Localizer["ct"],
            TargetType.TeamT => Instance.Localizer["t"],
            TargetType.TeamSpec => Instance.Localizer["spec"],
            _ => targetResult.Players.First().PlayerName
        };

        adminname = player?.PlayerName ?? Instance.Localizer["Console"];
        players = targetResult.Players;
        return true;
    }

    public static void SendMessageToAllPlayers(HudDestination destination, string messageKey, params object[] args)
    {
        LocalizedString message = Instance.Localizer[messageKey, args];
        VirtualFunctions.ClientPrintAll(destination, Instance.Config.Tag + message, 0, 0, 0, 0);
    }
}