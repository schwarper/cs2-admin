using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using System.Runtime.CompilerServices;
using System.Text;
using static CounterStrikeSharp.API.Modules.Commands.Targeting.Target;
using static PlayerCommands.PlayerCommands;

namespace PlayerCommands;

public static class Library
{
    public enum MultipleFlags
    {
        NORMAL = 0,
        IGNORE_DEAD_PLAYERS,
        IGNORE_ALIVE_PLAYERS
    }

    private static readonly Random Random = new();

    public static bool ProcessTargetString(
        CCSPlayerController? player,
        CommandInfo info,
        string targetstr,
        bool singletarget,
        bool immunitycheck,
        MultipleFlags flags,
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

        if (flags == MultipleFlags.IGNORE_DEAD_PLAYERS)
        {
            targetResult.Players.RemoveAll(target => !target.PawnIsAlive);

            if (targetResult.Players.Count == 0)
            {
                info.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["You can target only alive players"]);
                return false;
            }
        }
        else if (flags == MultipleFlags.IGNORE_ALIVE_PLAYERS)
        {
            targetResult.Players.RemoveAll(target => target.PawnIsAlive);

            if (targetResult.Players.Count == 0)
            {
                info.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["You can target only dead players"]);
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

    public static void Slap(this CBasePlayerPawn pawn, int damage = 0)
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

    public static void Rename(this CCSPlayerController player, string newname)
    {
        SchemaString<CBasePlayerController> playername = new(player, "m_iszPlayerName");
        playername.Set(newname);
        Utilities.SetStateChanged(player, "CBasePlayerController", "m_iszPlayerName");
    }
}