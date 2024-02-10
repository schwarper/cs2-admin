using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;

namespace Admin;

public static class PlayerUtils
{
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
        if(noclip)
        {
            pawn.MoveType = MoveType_t.MOVETYPE_NOCLIP;

            Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 8);
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        }
        else
        {
            pawn.MoveType = MoveType_t.MOVETYPE_WALK;

            Schema.SetSchemaValue(pawn.Handle, "CBaseEntity", "m_nActualMoveType", 2);
            Utilities.SetStateChanged(pawn, "CBaseEntity", "m_MoveType");
        }
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
        Admin.SchemaString<CBasePlayerController> playerName = new(player, "m_iszPlayerName");
        playerName.Set(newname);
    }
}