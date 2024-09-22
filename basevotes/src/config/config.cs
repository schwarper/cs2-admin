using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace BaseVotes;

public class Config : BasePluginConfig
{
    [JsonPropertyName("Tag")] public string Tag { get; set; } = "{red}[CSS] ";
}