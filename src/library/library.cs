using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using System.Drawing;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Text;
using static Admin.Admin;

namespace Admin;

public static class Library
{
    public static string GetCvarStringValue(ConVar cvar)
    {
        ConVarType cvartype = cvar.Type;

        return cvartype switch
        {
            ConVarType.Bool => cvar.GetPrimitiveValue<bool>().ToString(),
            ConVarType.Int16 => cvar.GetPrimitiveValue<Int16>().ToString(),
            ConVarType.UInt16 => cvar.GetPrimitiveValue<UInt16>().ToString(),
            ConVarType.Int32 => cvar.GetPrimitiveValue<int>().ToString(),
            ConVarType.UInt32 => cvar.GetPrimitiveValue<UInt32>().ToString(),
            ConVarType.Int64 => cvar.GetPrimitiveValue<Int64>().ToString(),
            ConVarType.UInt64 => cvar.GetPrimitiveValue<UInt64>().ToString(),
            ConVarType.Float32 => cvar.GetPrimitiveValue<float>().ToString(),
            ConVarType.Float64 => cvar.GetPrimitiveValue<double>().ToString(),
            ConVarType.String => cvar.StringValue,
            ConVarType.Color => cvar.GetPrimitiveValue<Color>().ToString(),
            ConVarType.Vector2 => cvar.GetPrimitiveValue<Vector2>().ToString(),
            ConVarType.Vector3 => cvar.GetPrimitiveValue<Vector3>().ToString(),
            ConVarType.Vector4 => cvar.GetPrimitiveValue<Vector4>().ToString(),
            ConVarType.Qangle => cvar.GetPrimitiveValue<QAngle>().ToString(),
            ConVarType.Invalid => "Invalid",
            _ => "Invalid"
        };
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

    public static void PrintToChatAll(string message, params object[] args)
    {
        object[] modifiedArgs;

        if (args.Length > 0 && args[0]?.ToString() == "Console")
        {
            if (Instance.Config.HideConsoleMsg)
            {
                return;
            }

            modifiedArgs = new object[args.Length];
            Array.Copy(args, modifiedArgs, args.Length);
            modifiedArgs[0] = Instance.Localizer["Console"];
        }
        else
        {
            modifiedArgs = args;
        }

        foreach (CCSPlayerController player in Utilities.GetPlayers())
        {
            if (Instance.Config.ShowNameCommands.Contains(message.Split('<').First()) && Instance.Config.ShowNameFlag != string.Empty && !AdminManager.PlayerHasPermissions(player, Instance.Config.ShowNameFlag))
            {
                if (args.Length > 0 && args[0]?.ToString() == "Console")
                {
                    modifiedArgs[0] = Instance.Localizer["Console"];
                }
                else
                {
                    modifiedArgs = new object[args.Length];
                    Array.Copy(args, modifiedArgs, args.Length);
                    modifiedArgs[0] = Instance.Localizer["Console"];
                }
            }

            using (new WithTemporaryCulture(player.GetLanguage()))
            {
                StringBuilder builder = new(Instance.Config.Tag);
                builder.AppendFormat(Instance.Localizer[message], modifiedArgs);
                player.PrintToChat(builder.ToString());
            }
        }
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
}