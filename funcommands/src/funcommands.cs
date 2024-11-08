
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using static FunCommands.Library;

namespace FunCommands;

public class FunCommands : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Fun Commands";
    public override string ModuleVersion => "1.7";
    public override string ModuleAuthor => "schwarper";
    public override string ModuleDescription => "Fun Commands";

    public static FunCommands Instance { get; set; } = new FunCommands();
    public Config Config { get; set; } = new Config();

    public override void Load(bool hotReload)
    {
        Instance = this;
    }

    public void OnConfigParsed(Config config)
    {
        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);
        Config = config;
    }

    [GameEventHandler]
    public HookResult OnPlayerSpawn(EventPlayerSpawn @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null || player.Team < CsTeam.Terrorist)
        {
            return HookResult.Continue;
        }

        player.RemoveAllTimers();
        player.RemoveLastCoord();

        player.PlayerPawn.Value?.Glow(Color.White);

        Instance.AddTimer(0.1f, () =>
        {
            if (!player.IsValid)
            {
                return;
            }

            CsTeam team = player.Team;

            if (team == CsTeam.CounterTerrorist)
            {
                player.Health(Instance.Config.CTDefaultHealth);
            }
            else if (team == CsTeam.Terrorist)
            {
                player.Health(Instance.Config.TDefaultHealth);
            }
        });

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDeath(EventPlayerDeath @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        player.RemoveAllTimers();
        player.CopyLastCoord();

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnPlayerDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        player.RemoveAllTimers();
        return HookResult.Continue;
    }


    [ConsoleCommand("css_beacon")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 2, usage: "<#userid|name|all @ commands> <value>")]
    public void Command_Beacon(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, false, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        if (!int.TryParse(args[1], out int value))
        {
            SendMessageToReplyToCommand(info, "Must be an integer");
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            if (value > 0)
            {
                target.AddTimer(TimerFlag.Beacon, AddTimer(3.0f, () => target.Beacon(), TimerFlags.REPEAT));
            }
            else
            {
                target.RemoveTimer(TimerFlag.Beacon);
            }
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_beacon<player>", adminname, targetname, value);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_beacon<multiple>", adminname, targetname, value);
        }
    }

    [ConsoleCommand("css_freeze")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands> <time>")]
    public void Command_Freeze(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        if (args.Length < 2 || !float.TryParse(args[1], out float value) || value <= 0.0)
        {
            value = -1.0f;
        }

        foreach (CCSPlayerController target in players)
        {
            target.Freeze(value);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_freeze<player>", adminname, targetname, value);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_freeze<multiple>", adminname, targetname, value);
        }
    }

    [ConsoleCommand("css_unfreeze")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>")]
    public void Command_UnFreeze(CCSPlayerController? player, CommandInfo info)
    {
        if (!ProcessTargetString(player, info, info.GetArg(1), false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.UnFreeze();
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_unfreeze<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_unfreeze<multiple>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_gravity")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<gravity>")]
    public void Command_Gravity(CCSPlayerController? player, CommandInfo info)
    {
        if (!int.TryParse(info.GetArg(1), out int value))
        {
            SendMessageToReplyToCommand(info, "Must be an integer");
            return;
        }

        ConVar? cvar = ConVar.Find("sv_gravity");

        if (cvar == null)
        {
            SendMessageToReplyToCommand(info, "Cvar is not found", "sv_gravity");
            return;
        }

        Server.ExecuteCommand($"sv_gravity {value}");

        string adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        SendMessageToAllPlayers(HudDestination.Chat, "Cvar changed", adminname, "sv_gravity", value);
    }

    [ConsoleCommand("css_revive")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>")]
    public void Command_Revive(CCSPlayerController? player, CommandInfo info)
    {
        if (!ProcessTargetString(player, info, info.GetArg(1), false, false, MultipleFlags.NORMAL, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.Respawn();
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_respawn<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_respawn<multiple>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_respawn")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>")]
    public void Command_Respawn(CCSPlayerController? player, CommandInfo info)
    {
        if (!ProcessTargetString(player, info, info.GetArg(1), false, false, MultipleFlags.IGNORE_ALIVE_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.Respawn();
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_respawn<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_respawn<multiple>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_noclip")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands> <value>")]
    public void Command_Noclip(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        int value = 0;

        if (args.Length > 1 && !int.TryParse(args[1], out value))
        {
            SendMessageToReplyToCommand(info, "Must be an integer");
            return;
        }
        else if (args.Length == 1)
        {
            CBasePlayerPawn? targetPawn = players[0].PlayerPawn.Value;

            if (targetPawn == null)
            {
                return;
            }

            value = targetPawn.MoveType == MoveType_t.MOVETYPE_NOCLIP ? 0 : 1;
        }

        bool noclip = Convert.ToBoolean(value);

        foreach (CBasePlayerPawn? targetPawn in players.Select(p => p.PlayerPawn.Value))
        {
            if (targetPawn == null)
            {
                continue;
            }

            targetPawn.Noclip(noclip);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_noclip<player>", adminname, targetname, value);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_noclip<multiple>", adminname, targetname, value);
        }
    }


    [ConsoleCommand("css_weapon")]
    [ConsoleCommand("css_give")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 2, "<#userid|name|all @ commands> <weapon>")]
    public void Command_Weapon(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, false, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        if (args[1].StartsWith("weapon_"))
        {
            args[1] = args[1][7..];
        }

        if (!GlobalWeaponDictionary.TryGetValue(args[1], out CsItem weaponname))
        {
            SendMessageToReplyToCommand(info, "Weapon is not exist");
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.GiveNamedItem(weaponname);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_weapon<player>", adminname, targetname, weaponname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_weapon<multiple>", adminname, targetname, weaponname);
        }
    }

    [ConsoleCommand("css_strip")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands> <slots>", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Strip(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        HashSet<gear_slot_t> slotsList = [];

        if (args.Length > 1)
        {
            foreach (char c in args[1])
            {
                if (GlobalSlotDictionary.TryGetValue(c, out gear_slot_t value))
                {
                    slotsList.Add(value);
                }
            }
        }

        if (slotsList.Count == 0)
        {
            slotsList = [gear_slot_t.GEAR_SLOT_RIFLE, gear_slot_t.GEAR_SLOT_PISTOL, gear_slot_t.GEAR_SLOT_GRENADES, gear_slot_t.GEAR_SLOT_C4];
        }

        foreach (CCSPlayerPawn? targetPawn in players.Select(p => p.PlayerPawn.Value))
        {
            Server.NextFrame(() => targetPawn?.Strip(slotsList));
        }

        string slotListStr = string.Join(", ", slotsList.Select(slot => slot.ToString().Replace("GEAR_SLOT_", "")));

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_strip<player>", adminname, targetname, slotListStr);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_strip<multiple>", adminname, targetname, slotListStr);
        }
    }

    [ConsoleCommand("css_hp")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 2, "<#userid|name|all @ commands> <health>")]
    public void Command_Hp(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        if (!int.TryParse(args[1], out int value))
        {
            SendMessageToReplyToCommand(info, "Must be an integer");
            return;
        }

        if (value <= 0)
        {
            SendMessageToReplyToCommand(info, "Must be higher than zero");
            return;
        }

        if (Config.MaxHealth != 0 && value > Config.MaxHealth)
        {
            value = Config.MaxHealth;
        }

        foreach (CCSPlayerController target in players)
        {
            target.Health(value);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_hp<player>", adminname, targetname, value);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_hp<multiple>", adminname, targetname, value);
        }
    }

    [ConsoleCommand("css_sethp")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 2, "<team> <health> - Sets team players' spawn health")]
    public void Command_SetHp(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        CsTeam team = args[0] switch
        {
            string s when s.ToUpper().StartsWith('T') => CsTeam.Terrorist,
            string s when s.ToUpper().StartsWith('C') => CsTeam.CounterTerrorist,
            "2" => CsTeam.Terrorist,
            "3" => CsTeam.CounterTerrorist,
            _ => CsTeam.None
        };

        if (team == CsTeam.None)
        {
            SendMessageToReplyToCommand(info, "No team exists");
            return;
        }

        if (!int.TryParse(args[1], out int value))
        {
            SendMessageToReplyToCommand(info, "Must be an integer");
            return;
        }

        if (value <= 0)
        {
            SendMessageToReplyToCommand(info, "Must be higher than zero");
            return;
        }

        if (team == CsTeam.CounterTerrorist)
        {
            Config.CTDefaultHealth = value;
        }
        else
        {
            Config.TDefaultHealth = value;
        }

        string adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        SendMessageToAllPlayers(HudDestination.Chat, "css_sethp", adminname, team, value);
    }

    [ConsoleCommand("css_speed")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 2, "<#userid|name|all @ commands> <value>")]
    public void Command_Speed(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        if (!float.TryParse(args[1], out float value))
        {
            SendMessageToReplyToCommand(info, "Must be an integer");
            return;
        }

        foreach (CCSPlayerPawn? targetPlayerPawn in players.Select(p => p.PlayerPawn.Value))
        {
            if (targetPlayerPawn == null)
            {
                continue;
            }

            targetPlayerPawn.Speed(value);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_speed<player>", adminname, targetname, value);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_speed<multiple>", adminname, targetname, value);
        }
    }

    [ConsoleCommand("css_god")]
    [ConsoleCommand("css_godmode")]
    [RequiresPermissions("@css/cheats")]
    [CommandHelper(minArgs: 2, "<#userid|name|all @ commands> <value>")]
    public void Command_God(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        if (!int.TryParse(args[1], out int value))
        {
            SendMessageToReplyToCommand(info, "Must be an integer");
            return;
        }

        value = Math.Max(0, Math.Min(1, value));

        bool godmode = Convert.ToBoolean(value);

        foreach (CCSPlayerPawn? targetPlayerPawn in players.Select(p => p.PlayerPawn.Value))
        {
            if (targetPlayerPawn == null)
            {
                continue;
            }

            targetPlayerPawn.Godmode(godmode);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_god<player>", adminname, targetname, value);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_god<multiple>", adminname, targetname, value);
        }
    }

    [ConsoleCommand("css_team")]
    [RequiresPermissions("@css/kick")]
    [CommandHelper(minArgs: 2, "<#userid|name|all @ commands> <value>")]
    public void Command_Team(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, false, MultipleFlags.NORMAL, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        string teamarg = args[1].ToLower();
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

        foreach (CCSPlayerController target in players)
        {
            target.ChangeTeam(team);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_team<player>", adminname, targetname, teamname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_team<multiple>", adminname, targetname, teamname);
        }
    }

    [ConsoleCommand("css_swap")]
    [RequiresPermissions("@css/kick")]
    [CommandHelper(minArgs: 1, "<#userid|name>")]
    public void Command_Swap(CCSPlayerController? player, CommandInfo info)
    {
        if (!ProcessTargetString(player, info, info.GetArg(1), false, false, MultipleFlags.NORMAL, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        CCSPlayerController target = players[0];

        string teamname;
        CsTeam team;

        if (target.Team == CsTeam.Terrorist)
        {
            teamname = "CT";
            team = CsTeam.CounterTerrorist;
        }
        else
        {
            teamname = "T";
            team = CsTeam.Terrorist;
        }

        target.SwitchTeam(team);

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_team<player>", adminname, targetname, teamname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_team<multiple>", adminname, targetname, teamname);
        }
    }

    [ConsoleCommand("css_bury")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>")]
    public void Command_Bury(CCSPlayerController? player, CommandInfo info)
    {
        if (!ProcessTargetString(player, info, info.GetArg(1), false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        foreach (CBasePlayerPawn? targetPawn in players.Select(p => p.Pawn.Value))
        {
            targetPawn?.Bury();
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_bury<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_bury<multiple>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_unbury")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>")]
    public void Command_UnBury(CCSPlayerController? player, CommandInfo info)
    {
        if (!ProcessTargetString(player, info, info.GetArg(1), false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        foreach (CBasePlayerPawn? targetPawn in players.Select(p => p.Pawn.Value))
        {
            targetPawn?.UnBury();
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_unbury<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_unbury<multiple>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_clean")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 0, "- Clean weapons on the ground")]
    public void Command_Clean(CCSPlayerController? player, CommandInfo info)
    {
        RemoveWeaponsOnTheGround();

        string adminname = player?.PlayerName ?? Instance.Localizer["Console"];

        SendMessageToAllPlayers(HudDestination.Chat, "css_clean", adminname);
    }

    [ConsoleCommand("css_goto")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name> - Teleport player to a player's position", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_Goto(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            return;
        }

        if (!ProcessTargetString(player, info, info.GetArg(1), true, false, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        CCSPlayerPawn? targetPlayerPawn = players[0].PlayerPawn.Value;
        CCSPlayerPawn? playerPlayerPawn = player.PlayerPawn.Value;

        if (targetPlayerPawn == null || playerPlayerPawn == null)
        {
            return;
        }

        playerPlayerPawn.TeleportToPlayer(targetPlayerPawn);

        SendMessageToAllPlayers(HudDestination.Chat, "css_goto", player.PlayerName, targetname);
    }

    [ConsoleCommand("css_bring")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands> - Teleport players to a player's position", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_Bring(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            return;
        }

        if (!ProcessTargetString(player, info, info.GetArg(1), false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        CCSPlayerPawn? playerPlayerPawn = player.PlayerPawn.Value;

        if (playerPlayerPawn == null)
        {
            return;
        }

        foreach (CCSPlayerPawn? targetPlayerPawn in players.Select(p => p.PlayerPawn.Value))
        {
            if (targetPlayerPawn == null)
            {
                continue;
            }

            targetPlayerPawn.TeleportToPlayer(playerPlayerPawn);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_bring<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_bring<multiple>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_hrespawn")]
    [ConsoleCommand("css_1up")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name> - Respawns a player in his last known death position.")]
    public void Command_HRespawn(CCSPlayerController? player, CommandInfo info)
    {
        if (!ProcessTargetString(player, info, info.GetArg(1), false, false, MultipleFlags.IGNORE_ALIVE_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        CCSPlayerController target = players[0];

        Vector lastCoord = target.GetLastCoord();

        CCSPlayerPawn? targetPawn = target.PlayerPawn.Value;

        if (targetPawn == null || targetPawn.AbsRotation == null)
        {
            return;
        }

        target.Respawn();
        targetPawn.Teleport(lastCoord, targetPawn.AbsRotation, targetPawn.AbsVelocity);

        SendMessageToAllPlayers(HudDestination.Chat, "css_hrespawn", adminname, targetname);
    }

    [ConsoleCommand("css_glow")]
    [ConsoleCommand("css_color")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands> <color>", whoCanExecute: CommandUsage.CLIENT_ONLY)]
    public void Command_Glow(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, false, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        Color color = Color.White;

        if (args.Length > 1)
        {
            if (!Enum.TryParse(args[1], true, out KnownColor knownColor))
            {
                SendMessageToReplyToCommand(info, "No color exists");
                return;
            }

            color = Color.FromKnownColor(knownColor);
        }

        foreach (CCSPlayerPawn? targetPlayerPawn in players.Select(p => p.PlayerPawn.Value))
        {
            if (targetPlayerPawn == null)
            {
                continue;
            }

            targetPlayerPawn.Glow(color);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_glow<player>", adminname, targetname, Localizer[color.Name]);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_glow<multiple>", adminname, targetname, Localizer[color.Name]);
        }
    }

    [ConsoleCommand("css_shake")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands> <time>")]
    public void Command_Shake(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        if (args.Length < 2 || !float.TryParse(args[1], out float value) || value <= 0.0)
        {
            value = 999f;
        }

        foreach (CCSPlayerController target in players)
        {
            target.Shake(value);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_shake<player>", adminname, targetname, value);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_shake<multiple>", adminname, targetname, value);
        }
    }

    [ConsoleCommand("css_unshake")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>")]
    public void Command_UnShake(CCSPlayerController? player, CommandInfo info)
    {
        if (!ProcessTargetString(player, info, info.GetArg(1), false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.UnShake();
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_unshake<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_unshake<multiple>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_blind")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands> <time>")]
    public void Command_Blind(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        if (args.Length < 2 || !float.TryParse(args[1], out float value) || value <= 0.0)
        {
            value = 999f;
        }

        foreach (CCSPlayerController target in players)
        {
            target.Blind(value);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_blind<player>", adminname, targetname, value);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_blind<multiple>", adminname, targetname, value);
        }
    }

    [ConsoleCommand("css_unblind")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, "<#userid|name|all @ commands>")]
    public void Command_UnBlind(CCSPlayerController? player, CommandInfo info)
    {
        if (!ProcessTargetString(player, info, info.GetArg(1), false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string adminname, out string targetname))
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.UnBlind();
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_unblind<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_unblind<multiple>", adminname, targetname);
        }
    }
}