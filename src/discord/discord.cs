using Newtonsoft.Json;
using System.Text;
using static Admin.Admin;

namespace Admin;

public static class Discord
{
    public static void SendMessage(string message)
    {
        if (string.IsNullOrEmpty(Instance.Config.DiscordWebhook))
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

        Task.Run(async () => await client.PostAsync(Instance.Config.DiscordWebhook, stringContent));
    }
}