using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace Admin;

public class AdminConfig : BasePluginConfig
{
    [JsonPropertyName("database")]
    public Dictionary<string, string> Database { get; set; } = new Dictionary<string, string>()
    {
        { "host", string.Empty },
        { "port", "3306" },
        { "user", string.Empty },
        { "password", string.Empty },
        { "name", string.Empty }
    };

    [JsonPropertyName("ca_tag_prefix")] public string Tag { get; set; } = "{red}[CSS]";
    [JsonPropertyName("ca_kick_delay")] public float KickDelay { get; set; } = 2.0f;
    [JsonPropertyName("ca_map_delay")] public float ChangeMapDelay { get; set; } = 2.0f;
    [JsonPropertyName("ca_sethp_max_health_100")] public bool SetHpMax100 { get; set; } = false;
    [JsonPropertyName("ca_ct_default_health")] public int CTDefaultHealth { get; set; } = 100;
    [JsonPropertyName("ca_t_default_health")] public int TDefaultHealth { get; set; } = 100;
    [JsonPropertyName("ca_hide_console_msg")] public bool HideConsoleMsg { get; set; } = true;
    [JsonPropertyName("ca_show_name_commands")] public string[] ShowNameCommands { get; set; } = ["css_slap", "css_slay"];
    [JsonPropertyName("ca_show_name_flag")] public string ShowNameFlag { get; set; } = "@css/generic";
    [JsonPropertyName("ca_discord_webhook")] public string DiscordWebhook { get; set; } = "";
    [JsonPropertyName("ca_workshop_map_name")] public Dictionary<string, ulong> WorkshopMapName { get; set; } = [];
}