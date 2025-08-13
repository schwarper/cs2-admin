using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace BaseChat;

public class Config : BasePluginConfig
{
    [JsonPropertyName("Tag")] public string Tag { get; set; } = "{red}[CSS] ";
}