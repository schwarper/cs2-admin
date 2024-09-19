using System.Text;
using System.Text.Json;
using static BaseBans.BaseBans;

namespace BaseBans;

public static class Discord
{
    private static readonly HttpClient _httpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(10)
    };

    private static readonly string BanTitle = Instance.Localizer["Discord ban title"];
    private static readonly string UnbanTitle = Instance.Localizer["Discord unban title"];
    private static readonly string AdminFieldName = Instance.Localizer["Discord admin field name"];
    private static readonly string PlayerFieldName = Instance.Localizer["Discord player field name"];
    private static readonly string DurationFieldName = Instance.Localizer["Discord duration field name"];
    private static readonly string EndFieldName = Instance.Localizer["Discord end field name"];
    private static readonly string ReasonFieldName = Instance.Localizer["Discord reason field name"];
    private static readonly string PermaBan = Instance.Localizer["Discord duration name perma"];

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public static async Task SendEmbedMessage(string playername, ulong playersteamid, string adminname, ulong adminsteamid, string reason, int duration, bool ban)
    {
        if (string.IsNullOrWhiteSpace(Instance.Config.DiscordWebhook))
        {
            return;
        }

        int color = ban ? (duration == 0 ? 16711680 : 16738740) : 3447003;
        string title = ban ? BanTitle : UnbanTitle;
        string adminvalue = adminsteamid == 0 ?
            $"**{adminname}**" :
            $"**[{adminname}](https://steamcommunity.com/profiles/{adminsteamid}) [{adminsteamid}]**";

        List<object> fields = [
            new { name = AdminFieldName, value = playersteamid == 0 ? adminname : adminvalue, inline = true },
            new { name = PlayerFieldName, value = $"**[{playername}](https://steamcommunity.com/profiles/{playersteamid}) [{playersteamid}]**", inline = true }
        ];

        if (ban)
        {
            fields.Add(new { name = '\u200b', value = '\u200b', inline = true });

            if (duration == 0)
            {
                fields.Add(new { name = DurationFieldName, value = $"`{PermaBan}`", inline = true });
            }
            else
            {
                fields.Add(new { name = DurationFieldName, value = $"`{duration}`", inline = true });
                fields.Add(new { name = EndFieldName, value = $"`{DateTime.Now.AddMinutes(duration):dd-MM-yyyy HH:mm:ss}`", inline = true });
            }
        }

        if (!string.IsNullOrWhiteSpace(reason))
        {
            fields.Add(new { name = ReasonFieldName, value = $"`{reason}`", inline = true });
        }

        if (fields.Count < 6)
        {
            fields.Add(new { name = '\u200b', value = '\u200b', inline = true });
        }

        var embedObject = new
        {
            embeds = new[]
            {
                new
                {
                    title,
                    color,
                    fields
                }
            }
        };

        string jsonString = JsonSerializer.Serialize(embedObject, JsonOptions);
        using StringContent stringContent = new(jsonString, Encoding.UTF8, "application/json");

        try
        {
            using HttpResponseMessage response = await _httpClient.PostAsync(Instance.Config.DiscordWebhook, stringContent).ConfigureAwait(false);
            string responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"HTTP request error: {response.StatusCode} - {responseContent}");
            }
        }
        catch (HttpRequestException httpRequestException)
        {
            Console.WriteLine($"HTTP request error: {httpRequestException.Message}");
        }
        catch (Exception generalException)
        {
            Console.WriteLine($"Error sending message to Discord: {generalException.Message}");
        }
    }
}
