using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace Admin;

public class AdminConfig : BasePluginConfig
{
    public class Config_Database
    {
        public bool UseMySql { get; set; } = true;
        public string Host { get; set; } = string.Empty;
        public uint Port { get; set; } = 3306;
        public string User { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    [JsonPropertyName("TagPrefix")] public string Tag { get; set; } = "{red}[CSS]";
    [JsonPropertyName("Database")] public Config_Database Database { get; set; } = new Config_Database();
    [JsonPropertyName("KickDelay")] public float KickDelay { get; set; } = 2.0f;
    [JsonPropertyName("MapDelay")] public float ChangeMapDelay { get; set; } = 2.0f;
    [JsonPropertyName("SetHpMax100")] public bool SetHpMax100 { get; set; } = false;
    [JsonPropertyName("CTDefaultHealth")] public int CTDefaultHealth { get; set; } = 100;
    [JsonPropertyName("TDefaultHealth")] public int TDefaultHealth { get; set; } = 100;
    [JsonPropertyName("HideConsoleMsg")] public bool HideConsoleMsg { get; set; } = true;
    [JsonPropertyName("ShowNameCommands")] public string[] ShowNameCommands { get; set; } = ["css_slap", "css_slay"];
    [JsonPropertyName("ShowNameFlag")] public string ShowNameFlag { get; set; } = "@css/generic";
    [JsonPropertyName("DiscordWebhook")] public string DiscordWebhook { get; set; } = "";
    [JsonPropertyName("WorkshopMapName")] public Dictionary<string, ulong> WorkshopMapName { get; set; } = [];
}