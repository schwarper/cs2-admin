using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Memory;
using System.Runtime.CompilerServices;
using System.Text;

namespace Admin;

public partial class Admin : BasePlugin
{
    private class Punishment
    {
        public ulong PlayerSteamid { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public ulong AdminSteamid { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public string PunishmentName { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public int Duration { get; set; }
        public DateTime End { get; set; }
        public DateTime Created { get; set; }
        public bool SaveDatabase { get; set; }
    }

    private class Target
    {
        public required CCSPlayerController[] Players { get; set; }
        public required string TargetName { get; set; }
    }

    public class SchemaString<SchemaClass> : NativeObject where SchemaClass : NativeObject
    {
        public SchemaString(SchemaClass instance, string member) : base(Schema.GetSchemaValue<nint>(instance.Handle, typeof(SchemaClass).Name!, member))
        { }

        public unsafe void Set(string str)
        {
            byte[] bytes = SchemaString<SchemaClass>.GetStringBytes(str);

            for (int i = 0; i < bytes.Length; i++)
            {
                Unsafe.Write((void*)(this.Handle.ToInt64() + i), bytes[i]);
            }

            Unsafe.Write((void*)(this.Handle.ToInt64() + bytes.Length), 0);
        }

        private static byte[] GetStringBytes(string str)
        {
            return Encoding.ASCII.GetBytes(str);
        }
    }
}