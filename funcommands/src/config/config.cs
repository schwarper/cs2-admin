using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace FunCommands;

public class Config : BasePluginConfig
{
    [JsonPropertyName("Tag")] public string Tag { get; set; } = "{red}[CSS] ";
    [JsonPropertyName("MaxHealth")] public int MaxHealth { get; set; } = 0;
    [JsonPropertyName("CTDefaultHealth")] public int CTDefaultHealth { get; set; } = 100;
    [JsonPropertyName("TDefaultHealth")] public int TDefaultHealth { get; set; } = 100;
}