using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace BaseCommTemp;

public class Config : BasePluginConfig
{
    public class Config_Database
    {
        public string Host { get; set; } = string.Empty;
        public uint Port { get; set; } = 3306;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    [JsonPropertyName("Tag")] public string Tag { get; set; } = "{red}[CSS] ";
    [JsonPropertyName("Database")] public Config_Database Database { get; set; } = new Config_Database();
}