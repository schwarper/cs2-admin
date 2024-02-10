using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Utils;
using Dapper;

namespace Admin;

public partial class Admin : BasePlugin
{
    private CCSPlayerController? FindTarget(CommandInfo command, int minArgCount)
    {
        if(command.ArgCount < minArgCount)
        {
            return null;
        }

        TargetResult targetresult = command.GetArgTargetResult(1);

        if (targetresult.Players.Count == 0)
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["No matching client"]);
            return null;
        }
        else if (targetresult.Players.Count > 1)
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["More than one client matched"]);
            return null;
        }

        CCSPlayerController target = targetresult.First();

        return target.IsValid ? target : null;
    }
    private Target? FindTargets(CCSPlayerController? player, CommandInfo command, MultipleFlags flags, int minArgCount)
    {
        if (command.ArgCount < minArgCount)
        {
            return null;
        }

        string arg = command.GetArg(1).ToLower();

        if (arg[0] == '@')
        {
            return arg[1] switch
            {
                'm' => HandlePlayer(player, command, flags),
                't' => HandleTeam(command, CsTeam.Terrorist, Localizer["t team players"], flags),
                'c' => HandleTeam(command,CsTeam.CounterTerrorist, Localizer["ct team players"], flags),
                'd' => HandleAliveDead(command, alive: false, Localizer["dead players"], flags),
                'a' => arg[3] == 'i' ? HandleAliveDead(command, alive: false, Localizer["dead players"], flags) : HandleAll(command, Localizer["all players"], flags),
                _ => HandlePlayer(FindTarget(command, minArgCount), command, flags)
            };
        }

        return HandlePlayer(FindTarget(command, minArgCount), command, flags);
    }

    private Target? HandlePlayer(CCSPlayerController? player, CommandInfo command, MultipleFlags flags)
    {
        if(player == null || !CheckFlags(player, flags))
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["No matching client"]);
            return null;
        }

        return new Target
        {
            Players = new CCSPlayerController[] { player },
            TargetName = player.PlayerName
        };
    }

    private Target? HandleTeam(CommandInfo command, CsTeam team, string targetname, MultipleFlags flags)
    {
        CCSPlayerController[] players = Utilities.GetPlayers().Where(target => target is { IsValid: true } && target.Team == team && CheckFlags(target, flags)).ToArray();

        if (players.Length == 1)
        {
            targetname = players.First().PlayerName;
        }
        else if(players.Length == 0)
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["No matching client"]);
            return null;
        }

        return new Target
        {
            Players = players,
            TargetName = players.Length == 1 ? players.First().PlayerName : targetname
        };
    }
    private Target? HandleAliveDead(CommandInfo command, bool alive, string targetname, MultipleFlags flags)
    {
        CCSPlayerController[] players = Utilities.GetPlayers().Where(target => target is { IsValid: true } && target.PawnIsAlive == alive && CheckFlags(target, flags)).ToArray();

        if (players.Length == 1)
        {
            targetname = players.First().PlayerName;
        }
        else if (players.Length == 0)
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["No matching client"]);
            return null;
        }

        return new Target
        {
            Players = players,
            TargetName = players.Length == 1 ? players.First().PlayerName : targetname
        };
    }
    private Target? HandleAll(CommandInfo command, string targetname, MultipleFlags flags)
    {
        CCSPlayerController[] players = Utilities.GetPlayers().Where(target => target is { IsValid: true } && CheckFlags(target, flags)).ToArray();

        if (players.Length == 1)
        {
            targetname = players.First().PlayerName;
        }
        else if (players.Length == 0)
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["No matching client"]);
            return null;
        }

        return new Target
        {
            Players = players,
            TargetName = players.Length == 1 ? players.First().PlayerName : targetname
        };
    }
    private static bool CheckFlags(CCSPlayerController player, MultipleFlags flags)
    {
        return flags switch
        {
            MultipleFlags.IGNORE_DEAD_PLAYERS => player.PawnIsAlive,
            MultipleFlags.IGNORE_ALIVE_PLAYERS => !player.PawnIsAlive,
            MultipleFlags.NORMAL => true,
            _ => true
        };
    }
}