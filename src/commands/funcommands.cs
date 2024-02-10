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
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_freeze<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, value]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_freeze<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, value]);
        }
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
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_unfreeze<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_unfreeze<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
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

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_cvar", player == null ? Localizer["Console"] : player.PlayerName, "sv_gravity", value]);
    }

    [ConsoleCommand("css_revive")]
    [ConsoleCommand("css_respawn")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Revive(CCSPlayerController? player, CommandInfo command)
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
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_respawn<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_respawn<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
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

        if(!int.TryParse(command.GetArg(2), out int value))
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["Must be an integer"]);
            return;
        }

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
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_noclip<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, value]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_noclip<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, value]);
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
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_weapon<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, weaponname]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_weapon<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, weaponname]);
        }
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
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_strip<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_strip<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
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
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_sethp<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, value]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_sethp<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, value]);
        }
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
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_speed<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, value]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_speed<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, value]);
        }
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
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_god<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, value]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_god<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, value]);
        }
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
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_team<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, teamname]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_team<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName, teamname]);
        }
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
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_bury<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_bury<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
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
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_unbury<player>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
        else
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_unbury<multiple>", player == null ? Localizer["Console"] : player.PlayerName, players.TargetName]);
        }
    }

    [ConsoleCommand("css_clean")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 0, "- Clean weapons on the ground", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Clean(CCSPlayerController? player, CommandInfo command)
    {
        RemoveWeaponsOnTheGround();
        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_clean", player == null ? Localizer["Console"] : player.PlayerName]);
    }
}