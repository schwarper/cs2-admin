using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using System.Drawing;
using static CounterStrikeSharp.API.Modules.Commands.Targeting.Target;
using static FunCommands.FunCommands;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace FunCommands;

public static class Library
{
    public enum MultipleFlags
    {
        NORMAL = 0,
        IGNORE_DEAD_PLAYERS,
        IGNORE_ALIVE_PLAYERS
    }

    public enum TimerFlag
    {
        Beacon,
        Freeze,
        Shake,
        Blind
    };

    public static Dictionary<CCSPlayerController, (float X, float Y, float Z)> GlobalHRespawnPlayers { get; set; } = [];
    private static readonly Dictionary<CCSPlayerController, CEnvShake> GlobalPlayerShakes = [];
    private static readonly Dictionary<CCSPlayerController, Dictionary<TimerFlag, Timer>> PlayerTimers = [];
    private const int lines = 20;
    private const float radiusIncrement = 10.0f;
    private const float initialRadius = 20.0f;

    public static void AddTimer(this CCSPlayerController player, TimerFlag timerflag, Timer timer)
    {
        player.RemoveTimer(timerflag);

        if (!PlayerTimers.TryGetValue(player, out Dictionary<TimerFlag, Timer>? timers))
        {
            timers = [];
            PlayerTimers[player] = timers;
        }

        timers[timerflag] = timer;
    }

    public static void RemoveTimer(this CCSPlayerController player, TimerFlag timerflag)
    {
        if (PlayerTimers.TryGetValue(player, out Dictionary<TimerFlag, Timer>? timers))
        {
            if (timers.TryGetValue(timerflag, out Timer? timer))
            {
                timer.Kill();
                timers.Remove(timerflag);

                if (timers.Count == 0)
                {
                    PlayerTimers.Remove(player);
                }
            }
        }
    }

    public static void RemoveAllTimers(this CCSPlayerController player)
    {
        if (PlayerTimers.TryGetValue(player, out Dictionary<TimerFlag, Timer>? timers))
        {
            foreach (Timer timer in timers.Values)
            {
                timer.Kill();
            }

            PlayerTimers.Remove(player);
        }
    }

    public static void Freeze(this CCSPlayerController player, float value)
    {
        CCSPlayerPawn? playerPawn = player.PlayerPawn.Value;

        if (playerPawn == null)
        {
            return;
        }

        ChangeMovetype(playerPawn, MoveType_t.MOVETYPE_OBSOLETE, Color.Green);

        if (value > 0.0)
        {
            Timer timer = Instance.AddTimer(value, () => player.UnFreeze());
            player.AddTimer(TimerFlag.Freeze, timer);
        }
    }
    public static void UnFreeze(this CCSPlayerController player)
    {
        CCSPlayerPawn? playerPawn = player.PlayerPawn.Value;

        if (playerPawn == null)
        {
            return;
        }

        ChangeMovetype(playerPawn, MoveType_t.MOVETYPE_WALK, Color.White);
        player.RemoveTimer(TimerFlag.Freeze);
    }
    public static void Noclip(this CBasePlayerPawn pawn, bool noclip)
    {
        ChangeMovetype(pawn, noclip ? MoveType_t.MOVETYPE_NOCLIP : MoveType_t.MOVETYPE_WALK, null);
    }
    public static void Health(this CCSPlayerController player, int health)
    {
        CCSPlayerPawn? playerPawn = player.PlayerPawn.Value;

        if (playerPawn == null)
        {
            return;
        }

        player.Health = health;
        playerPawn.Health = health;

        if (health > 100)
        {
            player.MaxHealth = health;
            playerPawn.MaxHealth = health;
        }

        Utilities.SetStateChanged(playerPawn, "CBaseEntity", "m_iHealth");
    }
    public static void Speed(this CCSPlayerPawn pawn, float speed)
    {
        pawn.VelocityModifier = speed;
    }
    public static void Godmode(this CCSPlayerPawn pawn, bool godmode)
    {
        pawn.TakesDamage = !godmode;
    }
    public static void Bury(this CBasePlayerPawn pawn)
    {
        Vector? absOrigin = pawn.AbsOrigin;

        Vector vector = new(absOrigin!.X, absOrigin.Y, absOrigin.Z - 10.0f);
        pawn.Teleport(vector, pawn.AbsRotation, pawn.AbsVelocity);
    }
    public static void UnBury(this CBasePlayerPawn pawn)
    {
        Vector? absOrigin = pawn.AbsOrigin;

        Vector vector = new(absOrigin!.X, absOrigin.Y, absOrigin.Z + 10.0f);
        pawn.Teleport(vector, pawn.AbsRotation, pawn.AbsVelocity);
    }
    public static void TeleportToPlayer(this CCSPlayerPawn playerPawn, CCSPlayerPawn targetPawn)
    {
        Vector? position = targetPawn.AbsOrigin;
        QAngle? angle = targetPawn.AbsRotation;

        if (position == null || angle == null)
        {
            return;
        }

        Vector velocity = targetPawn.AbsVelocity;

        playerPawn.Teleport(position, angle, velocity);
    }
    public static void Glow(this CBasePlayerPawn playerPawn, Color color)
    {
        playerPawn.RenderMode = RenderMode_t.kRenderTransColor;
        playerPawn.Render = color;

        Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
    }
    public static void Strip(this CCSPlayerPawn playerPawn, HashSet<gear_slot_t> slotsList)
    {
        List<CHandle<CBasePlayerWeapon>>? weapons = playerPawn.WeaponServices?.MyWeapons.ToList();

        if (weapons?.Count is not > 0)
        {
            return;
        }

        if (playerPawn.ItemServices?.Handle is not nint Handle)
        {
            return;
        }

        if (slotsList.Count == GlobalSlotDictionary.Count)
        {
            VirtualFunction.CreateVoid<nint>(Handle, GameData.GetOffset("CCSPlayer_ItemServices_RemoveWeapons"))(Handle);
            return;
        }

        CBasePlayerWeapon? activeWeapon = playerPawn.WeaponServices!.ActiveWeapon.Value;
        bool removeActiveWeapon = false;

        foreach (CBasePlayerWeapon? weapon in weapons.Select(w => w.Value))
        {
            if (weapon?.IsValid is not true ||
                !weapon.VisibleinPVS ||
                weapon.As<CCSWeaponBase>().VData?.GearSlot is not gear_slot_t slot ||
                !slotsList.Contains(slot)
                )
            {
                continue;
            }

            if (activeWeapon == weapon)
            {
                removeActiveWeapon = true;
                continue;
            }

            weapon.AddEntityIOEvent("Kill", weapon);
        }

        if (removeActiveWeapon)
        {
            Instance.AddTimer(0.1f, () =>
            {
                if (activeWeapon?.IsValid is not true)
                {
                    return;
                }

                VirtualFunction.CreateVoid<nint, nint>(Handle, GameData.GetOffset("CCSPlayer_ItemServices_DropActivePlayerWeapon"))(Handle, activeWeapon.Handle);
                activeWeapon.AddEntityIOEvent("Kill", activeWeapon, delay: 0.1f);
            });
        }
    }
    public static void Beacon(this CCSPlayerController player)
    {
        Vector? absOrigin = player.PlayerPawn.Value?.AbsOrigin;

        if (absOrigin == null)
        {
            return;
        }

        float step = (float)(2 * Math.PI) / lines;
        float angle = 0.0f;
        Color teamColor = player.TeamNum == 2 ? Color.Red : Color.Blue;

        List<CBeam> beams = [];

        for (int i = 0; i < lines; i++)
        {
            Vector start = CalculateCirclePoint(angle, initialRadius, absOrigin);
            angle += step;
            Vector end = CalculateCirclePoint(angle, initialRadius, absOrigin);

            CBeam? beam = CreateAndDrawBeam(start, end, teamColor, 1.0f, 2.0f);

            if (beam != null)
            {
                beams.Add(beam);
            }
        }

        float elapsed = 0.0f;

        Instance.AddTimer(0.1f, () =>
        {
            if (elapsed >= 0.9f)
            {
                return;
            }

            MoveBeams(beams, absOrigin, angle, step, radiusIncrement, elapsed);
            elapsed += 0.1f;
        }, TimerFlags.REPEAT);

        player.ExecuteClientCommand($"play sounds/tools/sfm/beep.vsnd_c");
    }
    public static void Shake(this CCSPlayerController player, float value)
    {
        player.UnShake();

        CEnvShake? entity = Utilities.CreateEntityByName<CEnvShake>("env_shake");

        if (entity == null || !entity.IsValid)
        {
            return;
        }

        CCSPlayerPawn? playerPawn = player.PlayerPawn.Value;

        if (playerPawn == null)
        {
            return;
        }

        Vector? absOrigin = playerPawn.AbsOrigin;

        if (absOrigin == null)
        {
            return;
        }

        entity.Amplitude = 10;
        entity.Frequency = 255;
        entity.Duration = value;
        entity.Radius = 50;

        entity.Teleport(new Vector(absOrigin.X, absOrigin.Y, absOrigin.Z + 72), QAngle.Zero, Vector.Zero);
        entity.DispatchSpawn();
        entity.AcceptInput("StartShake");
        entity.AcceptInput("SetParent", playerPawn, playerPawn, "!activator");

        GlobalPlayerShakes.Add(player, entity);

        if (value > 0.0)
        {
            Timer timer = Instance.AddTimer(value, () => player.UnShake());
            player.AddTimer(TimerFlag.Shake, timer);
        }
    }
    public static void UnShake(this CCSPlayerController player)
    {
        if (GlobalPlayerShakes.TryGetValue(player, out CEnvShake? entity))
        {
            entity.AcceptInput("StopShake");
            entity.Remove();

            GlobalPlayerShakes.Remove(player);

            player.RemoveTimer(TimerFlag.Shake);
        }
    }
    public static void Blind(this CCSPlayerController player, float value)
    {
        player.UnBlind();

        CCSPlayerPawn? playerPawn = player.PlayerPawn.Value;

        if (playerPawn == null)
        {
            return;
        }

        playerPawn.FlashMaxAlpha = 255;
        playerPawn.FlashDuration = 9999;
        playerPawn.BlindStartTime = Server.CurrentTime;
        playerPawn.BlindUntilTime = 9999;

        Utilities.SetStateChanged(playerPawn, "CCSPlayerPawnBase", "m_flFlashMaxAlpha");
        Utilities.SetStateChanged(playerPawn, "CCSPlayerPawnBase", "m_flFlashDuration");

        if (value > 0.0)
        {
            Timer timer = Instance.AddTimer(value, () => player.UnBlind());
            player.AddTimer(TimerFlag.Blind, timer);
        }
    }
    public static void UnBlind(this CCSPlayerController player)
    {
        CCSPlayerPawn? playerPawn = player.PlayerPawn.Value;

        if (playerPawn == null)
        {
            return;
        }

        if (playerPawn.BlindUntilTime == 9999)
        {
            playerPawn.BlindUntilTime = Server.CurrentTime - 1;
        }
    }

    public static void CopyLastCoord(this CCSPlayerController player)
    {
        Vector? absOrigin = player.PlayerPawn.Value?.AbsOrigin;

        if (absOrigin == null)
        {
            return;
        }

        if (GlobalHRespawnPlayers.ContainsKey(player))
        {
            GlobalHRespawnPlayers[player] = (absOrigin.X, absOrigin.Y, absOrigin.Z);
        }
        else
        {
            GlobalHRespawnPlayers.Add(player, (absOrigin.X, absOrigin.Y, absOrigin.Z));
        }
    }
    public static void RemoveLastCoord(this CCSPlayerController player)
    {
        GlobalHRespawnPlayers.Remove(player);
    }
    public static Vector GetLastCoord(this CCSPlayerController player)
    {
        (float X, float Y, float Z) = GlobalHRespawnPlayers.First(p => p.Key == player).Value;

        return new Vector(X, Y, Z);
    }

    public static void RemoveWeaponsOnTheGround()
    {
        IEnumerable<CCSWeaponBaseGun> entities = Utilities.FindAllEntitiesByDesignerName<CCSWeaponBaseGun>("weapon_");

        foreach (CCSWeaponBaseGun entity in entities)
        {
            if (!entity.IsValid)
            {
                continue;
            }

            if (entity.State != CSWeaponState_t.WEAPON_NOT_CARRIED)
            {
                continue;
            }

            if (entity.DesignerName.StartsWith("weapon_") == false)
            {
                continue;
            }

            entity.Remove();
        }
    }
    private static void ChangeMovetype(this CBasePlayerPawn pawn, MoveType_t movetype, Color? color)
    {
        pawn.MoveType = movetype;
        Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", movetype);
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");

        if (color != null)
        {
            pawn.Glow(Color.Green);
        }
    }
    private static Vector CalculateCirclePoint(float angle, float radius, Vector mid)
    {
        return new Vector(
            (float)(mid.X + (radius * Math.Cos(angle))),
            (float)(mid.Y + (radius * Math.Sin(angle))),
            mid.Z + 6.0f
        );
    }
    private static CBeam? CreateAndDrawBeam(Vector start, Vector end, Color color, float life, float width)
    {
        CBeam? beam = Utilities.CreateEntityByName<CBeam>("beam");

        if (beam != null)
        {
            beam.Render = color;
            beam.Width = width;
            beam.Teleport(start, new QAngle(), new Vector());
            beam.EndPos.X = end.X;
            beam.EndPos.Y = end.Y;
            beam.EndPos.Z = end.Z;
            beam.DispatchSpawn();
            Instance.AddTimer(life, () => beam.Remove());
        }

        return beam;
    }
    private static void MoveBeams(List<CBeam> beams, Vector mid, float angle, float step, float radiusIncrement, float elapsed)
    {
        float radius = initialRadius + (radiusIncrement * (elapsed / 0.1f));

        foreach (CBeam beam in beams)
        {
            Vector start = CalculateCirclePoint(angle, radius, mid);
            angle += step;
            Vector end = CalculateCirclePoint(angle, radius, mid);
            TeleportLaser(beam, start, end);
        }
    }
    private static void TeleportLaser(CBeam beam, Vector start, Vector end)
    {
        if (beam != null && beam.IsValid)
        {
            beam.Teleport(start, new QAngle(), new Vector());
            beam.EndPos.X = end.X;
            beam.EndPos.Y = end.Y;
            beam.EndPos.Z = end.Z;
            Utilities.SetStateChanged(beam, "CBeam", "m_vecEndPos");
        }
    }
    public static readonly Dictionary<string, CsItem> GlobalWeaponDictionary = new()
    {
        { "zeus", CsItem.Taser },
        { "taser", CsItem.Taser },
        { "snowball", CsItem.Snowball },
        { "shield", CsItem.Shield },
        { "c4", CsItem.C4 },
        { "healthshot", CsItem.Healthshot },
        { "breachcharge", CsItem.BreachCharge },
        { "tablet", CsItem.Tablet },
        { "bumpmine", CsItem.Bumpmine },
        { "smoke", CsItem.SmokeGrenade },
        { "smokegrenade", CsItem.SmokeGrenade },
        { "flash", CsItem.Flashbang },
        { "flashbang", CsItem.Flashbang },
        { "hg", CsItem.HEGrenade },
        { "he", CsItem.HEGrenade },
        { "hegrenade", CsItem.HEGrenade },
        { "molotov", CsItem.Molotov },
        { "inc", CsItem.IncendiaryGrenade },
        { "incgrenade", CsItem.IncendiaryGrenade },
        { "decoy", CsItem.Decoy },
        { "ta", CsItem.TAGrenade },
        { "tagrenade", CsItem.TAGrenade },
        { "frag", CsItem.Frag },
        { "firebomb", CsItem.Firebomb },
        { "diversion", CsItem.Diversion },
        { "knife_t", CsItem.KnifeT },
        { "knife", CsItem.Knife },
        { "deagle", CsItem.Deagle },
        { "glock", CsItem.Glock },
        { "usp", CsItem.USPS },
        { "usp_silencer", CsItem.USPS },
        { "hkp2000", CsItem.HKP2000 },
        { "elite", CsItem.Elite },
        { "tec9", CsItem.Tec9 },
        { "p250", CsItem.P250 },
        { "cz75a", CsItem.CZ75 },
        { "fiveseven", CsItem.FiveSeven },
        { "revolver", CsItem.Revolver },
        { "mac10", CsItem.Mac10 },
        { "mp9", CsItem.MP9 },
        { "mp7", CsItem.MP7 },
        { "p90", CsItem.P90 },
        { "mp5", CsItem.MP5SD },
        { "mp5sd", CsItem.MP5SD },
        { "bizon", CsItem.Bizon },
        { "ump45", CsItem.UMP45 },
        { "xm1014", CsItem.XM1014 },
        { "nova", CsItem.Nova },
        { "mag7", CsItem.MAG7 },
        { "sawedoff", CsItem.SawedOff },
        { "m249", CsItem.M249 },
        { "negev", CsItem.Negev },
        { "ak", CsItem.AK47 },
        { "ak47", CsItem.AK47 },
        { "m4s", CsItem.M4A1S },
        { "m4a1s", CsItem.M4A1S },
        { "m4a1_silencer", CsItem.M4A1S },
        { "m4", CsItem.M4A1 },
        { "m4a1", CsItem.M4A1 },
        { "galil", CsItem.Galil },
        { "galilar", CsItem.Galil },
        { "famas", CsItem.Famas },
        { "sg556", CsItem.SG556 },
        { "awp", CsItem.AWP },
        { "aug", CsItem.AUG },
        { "ssg08", CsItem.SSG08 },
        { "scar20", CsItem.SCAR20 },
        { "g3sg1", CsItem.G3SG1 },
        { "kevlar", CsItem.Kevlar },
        { "assaultsuit", CsItem.AssaultSuit }
    };

    public static readonly Dictionary<char, gear_slot_t> GlobalSlotDictionary = new()
    {
        {'1', gear_slot_t.GEAR_SLOT_RIFLE},
        {'2', gear_slot_t.GEAR_SLOT_PISTOL},
        {'3', gear_slot_t.GEAR_SLOT_KNIFE},
        {'4', gear_slot_t.GEAR_SLOT_GRENADES},
        {'5', gear_slot_t.GEAR_SLOT_C4}
    };

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
            SendMessageToReplyToCommand(info, "No matching client");
            return false;
        }
        else if (targetResult.Players.Count > 1)
        {
            if (singletarget || !TargetTypeMap.ContainsKey(targetstr))
            {
                SendMessageToReplyToCommand(info, "More than one client matched");
                return false;
            }
        }

        if (immunitycheck)
        {
            targetResult.Players.RemoveAll(target => !AdminManager.CanPlayerTarget(player, target));

            if (targetResult.Players.Count == 0)
            {
                SendMessageToReplyToCommand(info, "You cannot target");
                return false;
            }
        }

        if (flags == MultipleFlags.IGNORE_DEAD_PLAYERS)
        {
            targetResult.Players.RemoveAll(target => !target.PawnIsAlive);

            if (targetResult.Players.Count == 0)
            {
                SendMessageToReplyToCommand(info, "You can target only alive players");
                return false;
            }
        }
        else if (flags == MultipleFlags.IGNORE_ALIVE_PLAYERS)
        {
            targetResult.Players.RemoveAll(target => target.PawnIsAlive);

            if (targetResult.Players.Count == 0)
            {
                SendMessageToReplyToCommand(info, "You can target only dead players");
                return false;
            }
        }

        if (targetResult.Players.Count == 1)
        {
            targetname = targetResult.Players[0].PlayerName;
        }
        else
        {
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
        }

        adminname = player?.PlayerName ?? Instance.Localizer["Console"];
        players = targetResult.Players;
        return true;
    }

    public static void SendMessageToPlayer(CCSPlayerController player, HudDestination destination, string messageKey, params object[] args)
    {
        using (new WithTemporaryCulture(player.GetLanguage()))
        {
            LocalizedString message = Instance.Localizer[messageKey, args];
            VirtualFunctions.ClientPrint(player.Handle, destination, Instance.Config.Tag + message, 0, 0, 0, 0);
        }
    }

    public static void SendMessageToAllPlayers(HudDestination destination, string messageKey, params object[] args)
    {
        const string playerdesignername = "cs_player_controller";

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