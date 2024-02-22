using CounterStrikeSharp.API.Core;
using System.Text.Json.Serialization;

namespace Admin;

public class AdminConfig : BasePluginConfig
{
    [JsonPropertyName("database_host")] public string DatabaseHost { get; set; } = "";
    [JsonPropertyName("database_port")] public int DatabasePort { get; set; } = 3306;
    [JsonPropertyName("database_user")] public string DatabaseUser { get; set; } = "";
    [JsonPropertyName("database_password")] public string DatabasePassword { get; set; } = "";
    [JsonPropertyName("database_name")] public string DatabaseName { get; set; } = "";
    [JsonPropertyName("kick_delay")] public float KickDelay { get; set; } = 5.0f;
    [JsonPropertyName("changemap_delay")] public float ChangeMapDelay { get; set; } = 2.0f;
    [JsonPropertyName("give_knife_after_strip")] public bool GiveKnifeAfterStrip { get; set; } = true;
    [JsonPropertyName("sethp_max_100")] public bool SetHpMax100 { get; set; } = false;
    [JsonPropertyName("ct_default_health")] public int CTDefaultHealth { get; set; } = 100;
    [JsonPropertyName("t_default_health")] public int TDefaultHealth { get; set; } = 100;
    [JsonPropertyName("hide_console_msg")] public bool HideConsoleMsg { get; set; } = true;
    [JsonPropertyName("show_name_commands")] public string[] ShowNameCommands { get; set; } = { "css_slap", "css_slay" };
    [JsonPropertyName("show_name_flag")] public string ShowNameFlag { get; set; } = "@css/generic";
    [JsonPropertyName("workshop_map_name")] public Dictionary<string, ulong> WorkshopMapName { get; set; } = new();
    [JsonPropertyName("discord_webhook")] public string DiscordWebhook { get; set; } = "";
}

public partial class Admin : BasePlugin, IPluginConfig<AdminConfig>
{
    public void OnConfigParsed(AdminConfig config)
    {
        CreateDatabase(config);

        if (config.KickDelay <= 0.0)
        {
            config.KickDelay = 0.0f;
        }

        if (config.ChangeMapDelay <= 0.0)
        {
            config.ChangeMapDelay = 0.0f;
        }

        if (config.CTDefaultHealth <= 0)
        {
            config.CTDefaultHealth = 100;
        }

        if (config.TDefaultHealth <= 0)
        {
            config.TDefaultHealth = 100;
        }

        Config = config;
    }
}