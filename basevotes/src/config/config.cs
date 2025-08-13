using System.Text.Json.Serialization;
using CounterStrikeSharp.API.Core;

namespace BaseVotes;

public class Config : BasePluginConfig
{
    [JsonPropertyName("Tag")] public string Tag { get; set; } = "{red}[CSS] ";
}