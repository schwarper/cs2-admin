using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Utils;
using static BaseBans.BaseBans;

namespace BaseBans;

public static class Library
{
    private static readonly HttpClient _httpClient = new();

    public static bool ProcessTargetString(
        CCSPlayerController? player,
        CommandInfo info,
        string targetstr,
        out List<CCSPlayerController> players,
        out string adminname,
        out string targetname)
    {
        players = [];
        adminname = string.Empty;
        targetname = string.Empty;

        TargetResult targetResult = new Target(targetstr).GetTarget(player);

        if (targetResult.Players.Count == 0)
        {
            SendMessageToReplyToCommand(info, "No matching client");
            return false;
        }
        else if (targetResult.Players.Count > 1)
        {
            SendMessageToReplyToCommand(info, "More than one client matched");
            return false;
        }

        targetResult.Players.RemoveAll(target => !AdminManager.CanPlayerTarget(player, target));

        if (targetResult.Players.Count == 0)
        {
            SendMessageToReplyToCommand(info, "Unable to target");
            return false;
        }

        players = targetResult.Players;
        adminname = player?.PlayerName ?? Instance.Localizer["Console"];
        targetname = targetResult.Players[0].PlayerName;
        return true;
    }

    public static bool SteamIDTryParse(string id, out ulong steamId)
    {
        steamId = 0;

        if (id.Length != 17)
        {
            return false;
        }

        if (!ulong.TryParse(id, out steamId))
        {
            return false;
        }

        const ulong minSteamID = 76561197960265728;
        return steamId >= minSteamID;
    }

    public static async Task<string> GetPlayerNameFromSteamID(ulong steamID)
    {
        try
        {
            using HttpResponseMessage response = await _httpClient.GetAsync($"https://steamcommunity.com/profiles/{steamID}/?xml=1");
            response.EnsureSuccessStatusCode();

            string xmlContent = await response.Content.ReadAsStringAsync();

            System.Xml.XmlDocument xmlDoc = new();
            xmlDoc.LoadXml(xmlContent);

            System.Xml.XmlNode? nameNode = xmlDoc.SelectSingleNode("//steamID");

            string? name = nameNode?.InnerText.Trim();

            return string.IsNullOrWhiteSpace(name) ? steamID.ToString() : name;
        }
        catch (Exception)
        {
            return steamID.ToString();
        }
    }

    public static void SendMessageToPlayer(CCSPlayerController player, HudDestination destination, string messageKey, params object[] args)
    {
        player.PrintToChat(Instance.Config.Tag + Instance.Localizer.ForPlayer(player, messageKey, args));
    }

    public static void SendMessageToAllPlayers(HudDestination destination, string messageKey, params object[] args)
    {
        const string playerdesignername = "cs_player_controller";

        for (int i = 0; i < Server.MaxPlayers; i++)
        {
            CCSPlayerController? player = Utilities.GetEntityFromIndex<CCSPlayerController>(i + 1);

            if (player?.IsValid is not true || player.IsBot || player.DesignerName != playerdesignername || player.Connected != PlayerConnectedState.PlayerConnected)
            {
                continue;
            }

            SendMessageToPlayer(player, destination, messageKey, args);
        }
    }

    public static void SendMessageToReplyToCommand(CommandInfo info, string messageKey, params object[] args)
    {
        if (info.CallingPlayer == null)
        {
            Server.PrintToConsole(Instance.Config.Tag + Instance.Localizer[messageKey, args]);
        }
        else
        {
            SendMessageToPlayer(info.CallingPlayer,
                info.CallingContext == CommandCallingContext.Console ? HudDestination.Console : HudDestination.Chat,
                messageKey,
                args);
        }
    }
}