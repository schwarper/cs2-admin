using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Runtime.CompilerServices;
using System.Text;
using static CounterStrikeSharp.API.Modules.Commands.Targeting.Target;

namespace PlayerCommands;

public class PlayerCommands : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Player Commands";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "schwarper";

    public Config Config { get; set; } = new Config();
    private static Random Random = new Random();

    public void OnConfigParsed(Config config)
    {
        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);
        Config = config;
    }

    [ConsoleCommand("css_slap")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, usage: "<#userid|name|all @ commands> <damage>")]
    public void Command_Slap(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, false, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
        {
            return;
        }

        if (!int.TryParse(args[1], out int damage))
        {
            damage = 0;
        }

        foreach (CBasePlayerPawn? targetPawn in players.Select(p => p.Pawn.Value))
        {
            if (targetPawn == null)
            {
                continue;
            }

            Slap(targetPawn, damage);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_slap<player>", adminname, targetname, damage);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_slap<multiple>", adminname, targetname, damage);
        }
    }

    [ConsoleCommand("css_slay")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, usage: "<#userid|name|all @ commands>")]
    public void Command_Slay(CCSPlayerController? player, CommandInfo info)
    {
        if (!ProcessTargetString(player, info, info.GetArg(1), false, false, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
        {
            return;
        }

        foreach (CCSPlayerPawn? targetPlayerPawn in players.Select(p => p.PlayerPawn.Value))
        {
            if (targetPlayerPawn == null)
            {
                continue;
            }

            targetPlayerPawn.TakesDamage = true;
            targetPlayerPawn.CommitSuicide(false, true);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_slay<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_slay<player>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_rename")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 2, usage: "<#userid|name> <newname>")]
    public void Command_ReName(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], true, true, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
        {
            return;
        }

        CCSPlayerController target = players.Single();

        string newname = string.Join(" ", args[1..]);

        if (string.IsNullOrEmpty(newname))
        {
            info.ReplyToCommand(Config.Tag + Localizer["Must be a string"]);
            return;
        }

        SendMessageToAllPlayers(HudDestination.Chat, "css_rename", adminname, targetname, newname);

        Rename(target, newname);
    }

    private static void Slap(CBasePlayerPawn pawn, int damage = 0)
    {
        if (pawn.Health <= 0)
        {
            return;
        }

        pawn.Health -= damage;

        if (pawn.Health <= 0)
        {
            pawn.CommitSuicide(true, true);
            return;
        }

        Vector vel = new(pawn.AbsVelocity.X, pawn.AbsVelocity.Y, pawn.AbsVelocity.Z);

        vel.X += (Random.Next(180) + 50) * ((Random.Next(2) == 1) ? -1 : 1);
        vel.Y += (Random.Next(180) + 50) * ((Random.Next(2) == 1) ? -1 : 1);
        vel.Z += Random.Next(200) + 100;

        pawn.Teleport(pawn.AbsOrigin, pawn.AbsRotation, vel);
    }

    public class SchemaString<TSchemaClass>(TSchemaClass instance, string member) :
        NativeObject(Schema.GetSchemaValue<nint>(instance.Handle, typeof(TSchemaClass).Name, member))
        where TSchemaClass : NativeObject
    {
        public unsafe void Set(string str)
        {
            byte[] bytes = GetStringBytes(str);

            for (int i = 0; i < bytes.Length; i++)
            {
                Unsafe.Write((void*)(Handle.ToInt64() + i), bytes[i]);
            }

            Unsafe.Write((void*)(Handle.ToInt64() + bytes.Length), 0);
        }

        private static byte[] GetStringBytes(string str)
        {
            return Encoding.UTF8.GetBytes(str);
        }
    }

    public static void Rename(CCSPlayerController player, string newname)
    {
        SchemaString<CBasePlayerController> playername = new(player, "m_iszPlayerName");
        playername.Set(newname);
        Utilities.SetStateChanged(player, "CBasePlayerController", "m_iszPlayerName");
    }

    private void SendMessageToAllPlayers(HudDestination destination, string messageKey, params object[] args)
    {
        Microsoft.Extensions.Localization.LocalizedString message = Localizer[messageKey, args];
        VirtualFunctions.ClientPrintAll(destination, Config.Tag + message, 0, 0, 0, 0);
    }

    private bool ProcessTargetString(
        CCSPlayerController? player,
        CommandInfo info,
        string targetstr,
        bool singletarget,
        bool singleignoredead,
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
            info.ReplyToCommand(Config.Tag + Localizer["No matching client"]);
            return false;
        }
        else if (targetResult.Players.Count > 1)
        {
            if (!TargetTypeMap.ContainsKey(targetstr) || singletarget)
            {
                info.ReplyToCommand(Config.Tag + Localizer["More than one client matched"]);
                return false;
            }

            targetResult.Players.RemoveAll(target => !AdminManager.CanPlayerTarget(player, target) || !target.PawnIsAlive);

            if (targetResult.Players.Count == 0)
            {
                info.ReplyToCommand(Config.Tag + Localizer["Unable to targets"]);
                return false;
            }
        }
        else
        {
            CCSPlayerController target = targetResult.Players[0];

            if (!AdminManager.CanPlayerTarget(player, target) || singleignoredead && !target.PawnIsAlive)
            {
                info.ReplyToCommand(Config.Tag + Localizer["Unable to target"]);
                return false;
            }

            players = [target];
            adminname = player?.PlayerName ?? Localizer["Console"];
            targetname = target.PlayerName;
            return true;
        }

        TargetTypeMap.TryGetValue(targetstr, out TargetType type);

        adminname = player?.PlayerName ?? Localizer["Console"];
        targetname = type switch
        {
            TargetType.GroupAll => Localizer["all"],
            TargetType.GroupBots => Localizer["bots"],
            TargetType.GroupHumans => Localizer["humans"],
            TargetType.GroupAlive => Localizer["alive"],
            TargetType.GroupDead => Localizer["dead"],
            TargetType.GroupNotMe => Localizer["notme"],
            TargetType.PlayerMe => targetResult.Players[0].PlayerName,
            TargetType.TeamCt => Localizer["ct"],
            TargetType.TeamT => Localizer["t"],
            TargetType.TeamSpec => Localizer["spec"],
            _ => targetResult.Players[0].PlayerName
        };

        return true;
    }
}