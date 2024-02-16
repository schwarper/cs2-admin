using CounterStrikeSharp.API.Core;
using Newtonsoft.Json;
using System.Text;

namespace Admin;

public partial class Admin : BasePlugin
{
    public async Task SendDiscordMessage(string message)
    {
        if (string.IsNullOrEmpty(Config.DiscordWebhook))
        {
            return;
        }

        using HttpClient client = new();

        var payload = new
        {
            content = $"`{message}`"
        };

        string jsonPayload = JsonConvert.SerializeObject(payload);
        StringContent stringContent = new(jsonPayload, Encoding.UTF8, "application/json");

        HttpResponseMessage response = await client.PostAsync(Config.DiscordWebhook, stringContent);
    }
}