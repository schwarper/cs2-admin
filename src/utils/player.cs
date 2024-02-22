using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using static Admin.Admin;

namespace Admin;

public static class PlayerUtils
{
    static public bool Valid(this CCSPlayerController player)
    {
        return player.IsValid && player.SteamID.ToString().Length == 17;
    }
    static public bool CheckFlag(this CCSPlayerController player, MultipleFlags flags)
    {
        return flags switch
        {
            MultipleFlags.IGNORE_DEAD_PLAYERS => player.PawnIsAlive,
            MultipleFlags.IGNORE_ALIVE_PLAYERS => !player.PawnIsAlive,
            MultipleFlags.NORMAL => true,
            _ => true
        };
    }
    static public void Kick(this CCSPlayerController player, string reason)
    {
        Server.ExecuteCommand($"kickid \"{player.UserId}\" \"{reason}\";");
    }
    static public void Freeze(this CBasePlayerPawn pawn)
    {
        pawn.MoveType = MoveType_t.MOVETYPE_NONE;

        Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 0);
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
    }
    static public void UnFreeze(this CBasePlayerPawn pawn)
    {
        pawn.MoveType = MoveType_t.MOVETYPE_WALK;

        Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 2);
        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
    }
    static public void Noclip(this CBasePlayerPawn pawn, bool noclip)
    {
        if (noclip)
        {
            pawn.MoveType = MoveType_t.MOVETYPE_NOCLIP;

            Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 8);
        }
        else
        {
            pawn.MoveType = MoveType_t.MOVETYPE_WALK;

            Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 2);
        }

        Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
    }
    static public void Health(this CCSPlayerController player, int health)
    {
        if (player.PlayerPawn == null || player.PlayerPawn.Value == null)
        {
            return;
        }

        player.Health = health;
        player.PlayerPawn.Value.Health = health;

        if (health > 100)
        {
            player.MaxHealth = health;
            player.PlayerPawn.Value.MaxHealth = health;
        }

        Utilities.SetStateChanged(player.PlayerPawn.Value, "CBaseEntity", "m_iHealth");
    }
    static public void Speed(this CCSPlayerPawn pawn, float speed)
    {
        pawn.VelocityModifier = speed;
    }
    static public void Godmode(this CCSPlayerPawn pawn, bool godmode)
    {
        pawn.TakesDamage = !godmode;
    }
    static public void Bury(this CBasePlayerPawn pawn)
    {
        if (pawn?.AbsOrigin == null || pawn.AbsRotation == null)
        {
            return;
        }

        Vector newPos = new(pawn.AbsOrigin.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z - 10.0f);
        pawn.Teleport(newPos, pawn.AbsRotation, pawn.AbsVelocity);
    }
    static public void UnBury(this CBasePlayerPawn pawn)
    {
        if (pawn?.AbsOrigin == null || pawn.AbsRotation == null)
        {
            return;
        }

        Vector newPos = new(pawn.AbsOrigin.X, pawn.AbsOrigin.Y, pawn.AbsOrigin.Z + 10.0f);
        pawn.Teleport(newPos, pawn.AbsRotation, pawn.AbsVelocity);
    }
    static public void Slap(this CBasePlayerPawn pawn, int damage = 0)
    {
        if (pawn.Health <= 0)
        {
            return;
        }

        pawn.Health -= damage;

        if (pawn.Health <= 0)
        {
            pawn.CommitSuicide(false, true);
            return;
        }

        Random random = new();
        Vector vel = new(pawn.AbsVelocity.X, pawn.AbsVelocity.Y, pawn.AbsVelocity.Z);

        vel.X += ((random.Next(180) + 50) * ((random.Next(2) == 1) ? -1 : 1));
        vel.Y += ((random.Next(180) + 50) * ((random.Next(2) == 1) ? -1 : 1));
        vel.Z += random.Next(200) + 100;

        pawn.Teleport(pawn.AbsOrigin!, pawn.AbsRotation!, vel);
    }
    static public void Rename(this CCSPlayerController player, string newname)
    {
        SchemaString<CBasePlayerController> playerName = new(player, "m_iszPlayerName");
        playerName.Set(newname);

        Utilities.SetStateChanged(player, "CBasePlayerController", "m_iszPlayerName");
    }
    static public void TeleportToPlayer(this CCSPlayerPawn playerPawn, CCSPlayerPawn targetPawn)
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
    static public void Color(this CCSPlayerPawn playerPawn, Color color)
    {
        playerPawn.RenderMode = RenderMode_t.kRenderTransColor;
        playerPawn.Render = color;

        Utilities.SetStateChanged(playerPawn, "CBaseModelEntity", "m_clrRender");
    }
    static public void Circle(
        this CCSPlayerController player,
        float minRadius = 30f,
        int radiusIncrement = 6,
        float numberOfCircles = 15
    )
    {
        const int numPoints = (int)Math.PI * 15;

        float radius = minRadius;

        Vector[] GenerateOffsets(float currentRadius)
        {
            Vector[] offsets = new Vector[numPoints];
            float angle = 360f / numPoints;

            for (int i = 0; i < numPoints; i++)
            {
                float x = currentRadius * MathF.Cos(DegToRadian(angle * i));
                float y = currentRadius * MathF.Sin(DegToRadian(angle * i));

                offsets[i] = new Vector(x, y, 0);
            }

            return offsets;
        }

        float DegToRadian(float d)
        {
            return (float)(d * (Math.PI / 180));
        }

        void DrawLine(Vector start, Vector end, int i)
        {
            CBeam? line = Utilities.CreateEntityByName<CBeam>("beam");

            if (line == null)
            {
                return;
            }

            line.Render = System.Drawing.Color.Green;
            line.Width = 2.0f;

            line.Teleport(start, new QAngle(), new Vector());

            line.EndPos.X = end.X;
            line.EndPos.Y = end.Y;
            line.EndPos.Z = end.Z;

            Utilities.SetStateChanged(line, "CBeam", "m_vecEndPos");

            Plugin.AddTimer(0.1f, line.Remove);
        }

        void DrawCircle(CCSPlayerController player, Vector[] offsets)
        {
            CCSPlayerPawn? playerPawn = player.PlayerPawn.Value;

            if (playerPawn == null)
            {
                return;
            }

            Vector? position = playerPawn.AbsOrigin;

            if (position == null)
            {
                return;
            }

            for (int i = 0; i < numPoints; i++)
            {
                Vector start = position + offsets[i];
                Vector end = position + offsets[(i + 1) % offsets.Length];

                DrawLine(start, end, i);
            }
        }

        for (float i = 0; i < numberOfCircles; i++)
        {
            Plugin.AddTimer((float)(i * 0.11), () =>
            {
                Vector[] offsets = GenerateOffsets(radius);

                DrawCircle(player, offsets);

                radius += radiusIncrement;
            });
        }
    }
}