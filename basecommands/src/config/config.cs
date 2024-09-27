using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace BaseCommands;

public class Config : BasePluginConfig
{
    [JsonPropertyName("Tag")] public string Tag { get; set; } = "{red}[CSS] ";
    [JsonPropertyName("ChangeMapDelay")] public float ChangeMapDelay { get; set; } = 2.0f;

    [JsonPropertyName("WorkshopMapName")]
    public Dictionary<string, ulong> WorkshopMapName { get; set; } = new Dictionary<string, ulong>()
    {
        { "awp_lego_2", 3146105097 }
    };
}