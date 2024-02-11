using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;

namespace Admin;
public partial class Admin : BasePlugin
{
    [ConsoleCommand("css_freeze")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands> <time>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Freeze(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.IGNORE_DEAD_PLAYERS, 1);

        if (players == null)
        {
            return;
        }

        if(!float.TryParse(command.GetArg(2), out float value) || value <= 0.0)
        {
            value = -1.0f;
        }

        foreach (var targetPawn in players.Players.Select(p => p.Pawn.Value))
        {
            if(targetPawn == null)
            {
                continue;
            }

            targetPawn.Freeze();

            if (value > 0.0)
            {
                AddTimer(value, () =>
                {
                    targetPawn.UnFreeze();
                });
            }
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_freeze<player>", GetPlayerNameOrConsole(player), players.TargetName, value]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_freeze<multiple>", GetPlayerNameOrConsole(player), players.TargetName, value]);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_freeze <{command.GetArg(1)}> <{value}>");
    }

    [ConsoleCommand("css_unfreeze")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_UnFreeze(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.IGNORE_DEAD_PLAYERS, 1);

        if (players == null)
        {
            return;
        }

        foreach (var targetPawn in players.Players.Select(p => p.Pawn.Value))
        {
            if (targetPawn == null)
            {
                continue;
            }

            targetPawn.UnFreeze();
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_unfreeze<player>", GetPlayerNameOrConsole(player), players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_unfreeze<multiple>", GetPlayerNameOrConsole(player), players.TargetName]);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_unfreeze {command.GetArg(1)}");
    }

    [ConsoleCommand("css_gravity")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<gravity>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Gravity(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 1)
        {
            return;
        }

        if(!int.TryParse(command.GetArg(1), out int value))
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["Must be an integer"]);
            return;
        }

        ConVar? cvar = ConVar.Find("sv_gravity");

        if (cvar == null)
        {
            command.ReplyToCommand(Localizer["Cvar is not found", "sv_gravity"]);
            return;
        }

        Server.ExecuteCommand($"sv_gravity {value}");

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_cvar", GetPlayerNameOrConsole(player), "sv_gravity", value]);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_gravity <{value}>");
    }

    [ConsoleCommand("css_revive")]
    [ConsoleCommand("css_respawn")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Respawn(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, Config.RespawnOnlyDead ? MultipleFlags.IGNORE_ALIVE_PLAYERS : MultipleFlags.NORMAL, 1);

        if (players == null)
        {
            return;
        }

        foreach (var target in players.Players)
        {
            target.Respawn();
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_respawn<player>", GetPlayerNameOrConsole(player), players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_respawn<multiple>", GetPlayerNameOrConsole(player), players.TargetName]);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_respawn <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_noclip")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands> <value>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Noclip(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.IGNORE_DEAD_PLAYERS, 1);

        if (players == null)
        {
            return;
        }

        bool succeed = int.TryParse(command.GetArg(2), out int value);

        if(succeed)
        {
            value = Math.Max(0, Math.Min(1, value));

            bool noclip = Convert.ToBoolean(value);

            foreach (var targetPawn in players.Players.Select(p => p.Pawn.Value))
            {
                if (targetPawn == null)
                {
                    continue;
                }

                targetPawn.Noclip(noclip);
            }

            if (players.Players.Length == 1)
            {
                Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_noclip<player>", GetPlayerNameOrConsole(player), players.TargetName, value]);
            }
            else
            {
                Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_noclip<multiple>", GetPlayerNameOrConsole(player), players.TargetName, value]);
            }

            _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_noclip <{command.GetArg(1)}> <{value}>");
        }
        else
        {
            if (players.Players.Length != 1)
            {
                command.ReplyToCommand(Localizer["Prefix"] + Localizer["Must be an integer"]);
                return;
            }

            var targetPawn = players.Players.First().Pawn.Value;

            if (targetPawn == null)
            {
                return;
            }

            value = targetPawn.MoveType == MoveType_t.MOVETYPE_NOCLIP ? 0 : 1;

            targetPawn.Noclip(Convert.ToBoolean(value));

            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_noclip<player>", GetPlayerNameOrConsole(player), players.TargetName, value]);

            _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_noclip <{command.GetArg(1)}>");
        }
    }

    [ConsoleCommand("css_weapon")]
    [ConsoleCommand("css_give")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 2, "<#userid|name|all @ commands> <weapon>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Weapon(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.IGNORE_DEAD_PLAYERS, 2);

        if (players == null)
        {
            return;
        }

        string weaponname = command.GetArg(2);

        if (!GlobalWeaponAllList.Contains(weaponname))
        {
            switch (weaponname.ToLower())
            {
                case "kevlar":
                    {
                        weaponname = "item_kevlar";
                        break;
                    }
                case "assaultsuit":
                    {
                        weaponname = "item_assaultsuit";
                        break;
                    }
                default:
                    {
                        command.ReplyToCommand(Localizer["Prefix"] + Localizer["Weapon is not exist"]);
                        return;
                    }
            }
        }
        else
        {
            weaponname = "weapon_" + weaponname;
        }

        foreach (var target in players.Players)
        {
            target.GiveNamedItem(weaponname);
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_weapon<player>", GetPlayerNameOrConsole(player), players.TargetName, weaponname]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_weapon<multiple>", GetPlayerNameOrConsole(player), players.TargetName, weaponname]);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_weapon <{command.GetArg(1)}> <{weaponname}>");
    }

    [ConsoleCommand("css_strip")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Strip(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.IGNORE_DEAD_PLAYERS, 1);

        if (players == null)
        {
            return;
        }

        foreach (var target in players.Players)
        {
            target.RemoveWeapons();

            if (Config.GiveKnifeAfterStrip)
            {
                target.GiveNamedItem(CsItem.Knife);
            }
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_strip<player>", GetPlayerNameOrConsole(player), players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_strip<multiple>", GetPlayerNameOrConsole(player), players.TargetName]);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_strip <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_sethp")]
    [ConsoleCommand("css_hp")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 2, "<#userid|name|all @ commands> <health>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Hp(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.IGNORE_DEAD_PLAYERS, 3);

        if (players == null)
        {
            return;
        }

        if (!int.TryParse(command.GetArg(2), out int value))
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["Must be an integer"]);
            return;
        }

        if (value <= 0)
        {
            command.ReplyToCommand(Localizer["Must be higher than zero"]);
            return;
        }

        if (Config.SetHpMax100 && value > 100)
        {
            value = 100;
        }

        foreach (var target in players.Players)
        {
            target.Health(value);
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_sethp<player>", GetPlayerNameOrConsole(player), players.TargetName, value]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_sethp<multiple>", GetPlayerNameOrConsole(player), players.TargetName, value]);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_sethp <{command.GetArg(1)}> <{value}>");
    }

    [ConsoleCommand("css_speed")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 2, "<#userid|name|all @ commands> <value>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Speed(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.IGNORE_DEAD_PLAYERS, 2);

        if (players == null)
        {
            return;
        }

        if (!float.TryParse(command.GetArg(2), out float value))
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["Must be an integer"]);
            return;
        }

        foreach (var targetPlayerPawn in players.Players.Select(p => p.PlayerPawn.Value))
        {
            if (targetPlayerPawn == null)
            {
                continue;
            }

            targetPlayerPawn.Speed(value);
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_speed<player>", GetPlayerNameOrConsole(player), players.TargetName, value]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_speed<multiple>", GetPlayerNameOrConsole(player), players.TargetName, value]);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_speed <{command.GetArg(1)}> <{value}>");
    }

    [ConsoleCommand("css_god")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 2, "<#userid|name|all @ commands> <value>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_God(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.IGNORE_DEAD_PLAYERS, 3);

        if (players == null)
        {
            return;
        }

        if (!int.TryParse(command.GetArg(2), out int value))
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["Must be an integer"]);
            return;
        }

        value = Math.Max(0, Math.Min(1, value));

        bool godmode = Convert.ToBoolean(value);

        foreach (var targetPlayerPawn in players.Players.Select(p => p.PlayerPawn.Value))
        {
            if (targetPlayerPawn == null)
            {
                continue;
            }

            targetPlayerPawn.Godmode(godmode);
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_god<player>", GetPlayerNameOrConsole(player), players.TargetName, value]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_god<multiple>", GetPlayerNameOrConsole(player), players.TargetName, value]);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_god <{command.GetArg(1)}> <{value}>");
    }

    [ConsoleCommand("css_team")]
    [RequiresPermissions("@css/kick")]
    [CommandHelper(minArgs: 2, "<#userid|name|all @ commands> <value>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Team(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.NORMAL, 2);

        if (players == null)
        {
            return;
        }

        string teamarg = command.GetArg(2).ToLower();
        string teamname;
        CsTeam team;

        switch (teamarg[0])
        {
            case 'c':
                {
                    teamname = "CT";
                    team = CsTeam.CounterTerrorist;
                    break;
                }
            case '2':
                {
                    teamname = "CT";
                    team = CsTeam.CounterTerrorist;
                    break;
                }
            case 't':
                {
                    teamname = "T";
                    team = CsTeam.Terrorist;
                    break;
                }
            case '1':
                {
                    teamname = "T";
                    team = CsTeam.Terrorist;
                    break;
                }
            default:
                {
                    teamname = "SPEC";
                    team = CsTeam.Spectator;
                    break;
                }
        }

        foreach (var target in players.Players)
        {
            target.ChangeTeam(team);
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_team<player>", GetPlayerNameOrConsole(player), players.TargetName, teamname]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_team<multiple>", GetPlayerNameOrConsole(player), players.TargetName, teamname]);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_team <{command.GetArg(1)}> <{teamname}>");
    }

    [ConsoleCommand("css_bury")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Bury(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.IGNORE_DEAD_PLAYERS, 1);

        if (players == null)
        {
            return;
        }

        foreach (var targetPawn in players.Players.Select(p => p.Pawn.Value))
        {
            if (targetPawn == null)
            {
                continue;
            }

            targetPawn.Bury();
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_bury<player>", GetPlayerNameOrConsole(player), players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_bury<multiple>", GetPlayerNameOrConsole(player), players.TargetName]);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_bury <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_unbury")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_UnBury(CCSPlayerController? player, CommandInfo command)
    {
        Target? players = FindTargets(player, command, MultipleFlags.IGNORE_DEAD_PLAYERS, 1);

        if (players == null)
        {
            return;
        }

        foreach (var targetPawn in players.Players.Select(p => p.Pawn.Value))
        {
            if (targetPawn?.AbsOrigin == null || targetPawn.AbsRotation == null)
            {
                continue;
            }

            targetPawn.UnBury();
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_unbury<player>", GetPlayerNameOrConsole(player), players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_unbury<multiple>", GetPlayerNameOrConsole(player), players.TargetName]);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_unbury <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_clean")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 0, "- Clean weapons on the ground", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Clean(CCSPlayerController? player, CommandInfo command)
    {
        RemoveWeaponsOnTheGround();

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_clean", GetPlayerNameOrConsole(player)]);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_clean");
    }

    [ConsoleCommand("css_goto")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name> - Teleport player to a player's position", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_Goto(CCSPlayerController? player, CommandInfo command)
    {
        if(player == null)
        {
            return;
        }

        CCSPlayerController? target = FindTarget(command, MultipleFlags.IGNORE_DEAD_PLAYERS, 1);

        if(target == null)
        {
            return;
        }

        var targetPlayerPawn = target.PlayerPawn.Value;
        var playerPlayerPawn = player.PlayerPawn.Value;

        if (targetPlayerPawn == null || playerPlayerPawn == null)
        {
            return;
        }

        playerPlayerPawn.TeleportToPlayer(targetPlayerPawn);

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_goto", player.PlayerName, target.PlayerName]);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_goto <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_bring")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands> - Teleport players to a player's position", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_Bring(CCSPlayerController? player, CommandInfo command)
    {
        if(player == null)
        {
            return;
        }

        Target? players = FindTargets(player, command, MultipleFlags.IGNORE_DEAD_PLAYERS, 1);

        if (players == null)
        {
            return;
        }

        var playerPlayerPawn = player.PlayerPawn.Value;

        if(playerPlayerPawn == null)
        {
            return;
        }

        foreach (var targetPlayerPawn in players.Players.Select(p => p.PlayerPawn.Value))
        {
            if(targetPlayerPawn == null)
            {
                continue;
            }

            targetPlayerPawn.TeleportToPlayer(playerPlayerPawn);
        }

        if (players.Players.Length == 1)
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_bring<player>", GetPlayerNameOrConsole(player), players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_bring<multiple>", GetPlayerNameOrConsole(player), players.TargetName]);
        }

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_bring <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_hrespawn")]
    [ConsoleCommand("css_1up")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name> - Respawns a player in his last known death position.", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_HRespawn(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            return;
        }

        CCSPlayerController? target = FindTarget(command, MultipleFlags.IGNORE_ALIVE_PLAYERS, 1);

        if (target == null)
        {
            return;
        }

        var targetPawn = target.PlayerPawn.Value;

        if(targetPawn == null || targetPawn.AbsRotation == null)
        {
            return;
        }

        Vector position = GlobalHRespawnPlayers.First(p => p.Key == target).Value;

        target.Respawn();
        targetPawn.Teleport(position, targetPawn.AbsRotation, targetPawn.AbsVelocity);

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_hrespawn", GetPlayerNameOrConsole(player), target.PlayerName]);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_hrespawn <{command.GetArg(1)}>");
    }
}