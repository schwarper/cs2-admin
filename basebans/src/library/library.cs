using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Memory;
using CounterStrikeSharp.API.Modules.Utils;
using Microsoft.Extensions.Localization;
using static BaseBans.BaseBans;
using static CounterStrikeSharp.API.Modules.Commands.Targeting.Target;

namespace BaseBans;

public static class Library
{
    public enum MultipleFlags
    {
        NORMAL = 0,
        IGNORE_DEAD_PLAYERS,
        IGNORE_ALIVE_PLAYERS
    }

    private static readonly HttpClient _httpClient = new();

    public static bool ProcessTargetString(
        CCSPlayerController? player,
        CommandInfo info,
        string targetstr,
        bool singletarget,
        bool immunitycheck,
        MultipleFlags flags,
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
            info.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["No matching client"]);
            return false;
        }
        else if (targetResult.Players.Count > 1)
        {
            if (singletarget || !TargetTypeMap.ContainsKey(targetstr))
            {
                info.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["More than one client matched"]);
                return false;
            }
        }

        if (immunitycheck)
        {
            targetResult.Players.RemoveAll(target => !AdminManager.CanPlayerTarget(player, target));

            if (targetResult.Players.Count == 0)
            {
                info.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["You cannot target"]);
                return false;
            }
        }

        if (flags == MultipleFlags.IGNORE_DEAD_PLAYERS)
        {
            targetResult.Players.RemoveAll(target => !target.PawnIsAlive);

            if (targetResult.Players.Count == 0)
            {
                info.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["You can target only alive players"]);
                return false;
            }
        }
        else if (flags == MultipleFlags.IGNORE_ALIVE_PLAYERS)
        {
            targetResult.Players.RemoveAll(target => target.PawnIsAlive);

            if (targetResult.Players.Count == 0)
            {
                info.ReplyToCommand(Instance.Config.Tag + Instance.Localizer["You can target only dead players"]);
                return false;
            }
        }

        TargetTypeMap.TryGetValue(targetstr, out TargetType type);

        targetname = type switch
        {
            TargetType.GroupAll => Instance.Localizer["all"],
            TargetType.GroupBots => Instance.Localizer["bots"],
            TargetType.GroupHumans => Instance.Localizer["humans"],
            TargetType.GroupAlive => Instance.Localizer["alive"],
            TargetType.GroupDead => Instance.Localizer["dead"],
            TargetType.GroupNotMe => Instance.Localizer["notme"],
            TargetType.PlayerMe => targetResult.Players.First().PlayerName,
            TargetType.TeamCt => Instance.Localizer["ct"],
            TargetType.TeamT => Instance.Localizer["t"],
            TargetType.TeamSpec => Instance.Localizer["spec"],
            _ => targetResult.Players.First().PlayerName
        };

        adminname = player?.PlayerName ?? Instance.Localizer["Console"];
        players = targetResult.Players;
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

            if (string.IsNullOrWhiteSpace(name))
            {
                return steamID.ToString();
            }

            return name;
        }
        catch (Exception)
        {
            return steamID.ToString();
        }
    }

    public static void SendMessageToAllPlayers(HudDestination destination, string messageKey, params object[] args)
    {
        LocalizedString message = Instance.Localizer[messageKey, args];
        VirtualFunctions.ClientPrintAll(destination, Instance.Config.Tag + message, 0, 0, 0, 0);
    }
}