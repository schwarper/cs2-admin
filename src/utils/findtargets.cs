using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;

namespace Admin;

public partial class Admin : BasePlugin
{
    private (List<CCSPlayerController> players, string targetname) FindTarget
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
            return (new List<CCSPlayerController>(), string.Empty);
        }

        TargetResult targetresult = command.GetArgTargetResult(1);

        if (targetresult.Players.Count == 0)
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["No matching client"]);
            return (new List<CCSPlayerController>(), string.Empty);
        }
        else if (singletarget && targetresult.Players.Count > 1)
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["More than one client matched"]);
            return (new List<CCSPlayerController>(), string.Empty);
        }

        if (immunitycheck)
        {
            targetresult.Players.RemoveAll(target => !AdminManager.CanPlayerTarget(player, target));

            if (targetresult.Players.Count == 0)
            {
                command.ReplyToCommand(Localizer["Prefix"] + Localizer["You cannot target"]);
                return (new List<CCSPlayerController>(), string.Empty);
            }
        }

        if (flags == MultipleFlags.IGNORE_DEAD_PLAYERS)
        {
            targetresult.Players.RemoveAll(target => !target.PawnIsAlive);

            if (targetresult.Players.Count == 0)
            {
                command.ReplyToCommand(Localizer["Prefix"] + Localizer["You can target only dead players"]);
                return (new List<CCSPlayerController>(), string.Empty);
            }
        }
        else if (flags == MultipleFlags.IGNORE_ALIVE_PLAYERS)
        {
            targetresult.Players.RemoveAll(target => target.PawnIsAlive);

            if (targetresult.Players.Count == 0)
            {
                command.ReplyToCommand(Localizer["Prefix"] + Localizer["You can target only alive players"]);
                return (new List<CCSPlayerController>(), string.Empty);
            }
        }

        string targetname;

        if (targetresult.Players.Count == 1)
        {
            targetname = targetresult.Players.Single().PlayerName;
        }
        else
        {
            TargetTypeMap.TryGetValue(command.GetArg(1), out TargetType type);

            targetname = type switch
            {
                TargetType.GroupAll => Localizer["all"],
                TargetType.GroupBots => Localizer["bots"],
                TargetType.GroupHumans => Localizer["humans"],
                TargetType.GroupAlive => Localizer["alive"],
                TargetType.GroupDead => Localizer["dead"],
                TargetType.GroupNotMe => Localizer["notme"],
                TargetType.PlayerMe => targetresult.Players.First().PlayerName,
                TargetType.TeamCt => Localizer["ct"],
                TargetType.TeamT => Localizer["t"],
                TargetType.TeamSpec => Localizer["spec"],
                _ => targetresult.Players.First().PlayerName
            };
        }

        return (targetresult.Players, targetname);
    }
}