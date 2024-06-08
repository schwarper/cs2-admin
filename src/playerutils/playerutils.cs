using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Timers;
using CounterStrikeSharp.API.Modules.Utils;
using static Admin.Admin;
using static Admin.Library;
using Color = System.Drawing.Color;

namespace Admin;

public static class PlayerUtils
{
    private static readonly Random Random = new();
    private const int lines = 20;
    private const float radiusIncrement = 10.0f;
    private const float initialRadius = 20.0f;

    private static void ChangeMovetype(this CBasePlayerPawn pawn, MoveType_t movetype)
    {
        pawn.MoveType = movetype;
        Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", movetype);
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
    }
    public static void Freeze(this CBasePlayerPawn pawn)
    {
        ChangeMovetype(pawn, MoveType_t.MOVETYPE_OBSOLETE);
        pawn.Glow(Color.Green);
    }
    public static void UnFreeze(this CBasePlayerPawn pawn)
    {
        ChangeMovetype(pawn, MoveType_t.MOVETYPE_WALK);
        pawn.Glow(Color.White);
    }
    public static void Noclip(this CBasePlayerPawn pawn, bool noclip)
    {
        ChangeMovetype(pawn, noclip ? MoveType_t.MOVETYPE_NOCLIP : MoveType_t.MOVETYPE_WALK);
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
    public static void Rename(this CCSPlayerController player, string newname)
    {
        SchemaString<CBasePlayerController> playerName = new(player, "m_iszPlayerName");
        playerName.Set(newname);
        Utilities.SetStateChanged(player, "CBasePlayerController", "m_iszPlayerName");
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
    public static void Strip(this CCSPlayerController player, List<gear_slot_t> slotList)
    {
        NetworkedVector<CHandle<CBasePlayerWeapon>>? myweapons = player.PlayerPawn.Value?.WeaponServices?.MyWeapons;

        if (myweapons == null)
        {
            return;
        }

        List<string> weaponList = [];

        foreach (CHandle<CBasePlayerWeapon> myweapon in myweapons)
        {
            CBasePlayerWeapon? weapon = myweapon.Value;

            if (weapon == null || !weapon.IsValid)
            {
                continue;
            }

            CCSWeaponBaseVData? _weapon = weapon.As<CCSWeaponBase>().VData;

            if (_weapon == null)
            {
                continue;
            }

            /*
             * TO DO
             * NEED T0 TEST
             * NEED TO FIND ANOTHER WAY
            if (slotList.Any(slot => slot == _weapon.GearSlot))
            {
                player.PlayerPawn.Value.WeaponServices.ActiveWeapon.Raw = myweapon.Raw;

                player.DropActiveWeapon();
                weapon.Remove();
            }

            
            if (slotList.Any(slot => slot == _weapon.GearSlot))
            {
                weapon.Remove();
            }
            */

            if (!slotList.Any(slot => slot == _weapon.GearSlot))
            {
                weaponList.Add(weapon.DesignerName);
            }
        }

        player.RemoveWeapons();

        foreach (string weapon in weaponList)
        {
            player.GiveNamedItem(weapon);
        }
    }

    public static void Kick(this CCSPlayerController player)
    {
        Instance.AddTimer(Instance.Config.KickDelay, () =>
        {
            Server.ExecuteCommand($"kickid \"{player.UserId}\";");
        });
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
        float radius = initialRadius + radiusIncrement * (elapsed / 0.1f);
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
}