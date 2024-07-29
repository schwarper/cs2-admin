using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using static Admin.Admin;

namespace Admin;

public static class FindTarget
{
    public static (List<CCSPlayerController> players, string adminname, string targetname) Find
        (
            CCSPlayerController? player,
            CommandInfo command,
            int minArgCount,
            bool singletarget,
            bool immunitycheck,
            MultipleFlags flags
        )
    {
        if (command.ArgCount < minArgCount)
        {
            return ([], string.Empty, string.Empty);
        }

        TargetResult targetresult = command.GetArgTargetResult(1);

        if (targetresult.Players.Count == 0)
        {
            command.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["No matching client"]);
            return ([], string.Empty, string.Empty);
        }
        else if (singletarget && targetresult.Players.Count > 1)
        {
            command.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["More than one client matched"]);
            return ([], string.Empty, string.Empty);
        }
        else if (!command.GetArg(1).StartsWith('@') && targetresult.Players.Count > 1)
        {
            command.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["More than one client matched"]);
            return ([], string.Empty, string.Empty);
        }

        if (immunitycheck)
        {
            targetresult.Players.RemoveAll(target => !AdminManager.CanPlayerTarget(player, target));

            if (targetresult.Players.Count == 0)
            {
                command.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["You cannot target"]);
                return ([], string.Empty, string.Empty);
            }
        }

        if (flags == MultipleFlags.IGNORE_DEAD_PLAYERS)
        {
            targetresult.Players.RemoveAll(target => !target.PawnIsAlive);

            if (targetresult.Players.Count == 0)
            {
                command.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["You can target only alive players"]);
                return ([], string.Empty, string.Empty);
            }
        }
        else if (flags == MultipleFlags.IGNORE_ALIVE_PLAYERS)
        {
            targetresult.Players.RemoveAll(target => target.PawnIsAlive);

            if (targetresult.Players.Count == 0)
            {
                command.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["You can target only dead players"]);
                return ([], string.Empty, string.Empty);
            }
        }

        string targetname;

        if (targetresult.Players.Count == 1)
        {
            targetname = targetresult.Players.Single().PlayerName;
        }
        else
        {
            Target.TargetTypeMap.TryGetValue(command.GetArg(1), out TargetType type);

            targetname = type switch
            {
                TargetType.GroupAll => Instance.Localizer["all"],
                TargetType.GroupBots => Instance.Localizer["bots"],
                TargetType.GroupHumans => Instance.Localizer["humans"],
                TargetType.GroupAlive => Instance.Localizer["alive"],
                TargetType.GroupDead => Instance.Localizer["dead"],
                TargetType.GroupNotMe => Instance.Localizer["notme"],
                TargetType.PlayerMe => targetresult.Players.First().PlayerName,
                TargetType.TeamCt => Instance.Localizer["ct"],
                TargetType.TeamT => Instance.Localizer["t"],
                TargetType.TeamSpec => Instance.Localizer["spec"],
                _ => targetresult.Players.First().PlayerName
            };
        }

        string adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        return (targetresult.Players, adminname, targetname);
    }

    public enum MultipleFlags
    {
        NORMAL = 0,
        IGNORE_DEAD_PLAYERS,
        IGNORE_ALIVE_PLAYERS
    }
}