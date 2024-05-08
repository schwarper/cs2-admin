using Discord.Webhook;
using static Admin.Admin;

namespace Admin;

public static class Discord
{
    private static DiscordWebhookClient? _client;

    public static void Create()
    {
        if (string.IsNullOrEmpty(Instance.Config.DiscordWebhook))
        {
            return;
        }

        _client = new(Instance.Config.DiscordWebhook);
    }

    public static void SendMessage(string message)
    {
        if (_client == null)
        {
            return;
        }

        _client.SendMessageAsync(message);
    }
}