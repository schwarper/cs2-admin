﻿using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using System.Drawing;
using System.Numerics;
using static BaseCommands.BaseCommands;

namespace BaseCommands;

public static class Library
{
    public static void SendMessageToAllPlayers(HudDestination destination, string messageKey, params object[] args)
    {
        LocalizedString message = Instance.Localizer[messageKey, args];
        VirtualFunctions.ClientPrintAll(destination, Instance.Config.Tag + message, 0, 0, 0, 0);
    }

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
}