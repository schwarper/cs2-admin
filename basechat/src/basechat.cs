using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Utils;
using static BaseChat.Library;

namespace BaseChat;

public class BaseChat : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Basic Chat";
    public override string ModuleVersion => "1.3";
    public override string ModuleAuthor => "schwarper";
    public override string ModuleDescription => "Basic Communication Commands";

    public static BaseChat Instance { get; set; } = new BaseChat();
    public Config Config { get; set; } = new Config();

    public override void Load(bool hotReload)
    {
        Instance = this;
    }

    public void OnConfigParsed(Config config)
    {
        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);
        Config = config;
    }

    [ConsoleCommand("css_say")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, usage: "sends message to all players")]
    public void Command_Say(CCSPlayerController? player, CommandInfo info)
    {
        string adminname = player?.PlayerName ?? Localizer["Console"];
        string message = info.ArgString;

        SendMessageToAllPlayers(HudDestination.Chat, "css_say", adminname, message);
    }

    [ConsoleCommand("css_csay")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<message> - sends centered message to all players")]
    public void Command_CSay(CCSPlayerController? player, CommandInfo info)
    {
        string adminname = player?.PlayerName ?? Localizer["Console"];
        string message = info.ArgString;

        SendMessageToAllPlayers(HudDestination.Center, "css_csay", adminname, message);
    }

    [ConsoleCommand("css_hsay")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<message>  - sends hud message to all players")]
    public void Command_HSay(CCSPlayerController? player, CommandInfo info)
    {
        string adminname = player?.PlayerName ?? Localizer["Console"];
        string message = info.ArgString;

        SendMessageToAllPlayers(HudDestination.Alert, "css_hsay", adminname, message);
    }

    [ConsoleCommand("css_asay")]
    [ConsoleCommand("css_chat")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 1, "<message> - sends message to admins")]
    public void Command_ASay(CCSPlayerController? player, CommandInfo info)
    {
        string adminname = player?.PlayerName ?? Localizer["Console"];
        string message = info.ArgString;
        const string playerdesignername = "cs_player_controller";

        for (int i = 0; i < Server.MaxPlayers; i++)
        {
            CCSPlayerController? target = Utilities.GetEntityFromIndex<CCSPlayerController>(i + 1);

            if (target?.DesignerName != playerdesignername)
            {
                continue;
            }

            if (!AdminManager.PlayerHasPermissions(target.AuthorizedSteamID, "@css/chat"))
            {
                continue;
            }

            SendMessageToPlayer(target, HudDestination.Chat, "css_asay", adminname, message);
        }
    }

    [ConsoleCommand("css_psay")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(minArgs: 2, usage: "<#userid|name> <message> - sends public message")]
    public void Command_PSay(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        TargetResult targetResult = new Target(args[0]).GetTarget(player);

        if (targetResult.Players.Count == 0)
        {
            SendMessageToReplyToCommand(info, true, "No matching client");
            return;
        }
        else if (targetResult.Players.Count > 1)
        {
            SendMessageToReplyToCommand(info, true, "More than one client matched");
            return;
        }

        CCSPlayerController target = targetResult.Players[0];

        string targetname = target.PlayerName;
        string adminname = player?.PlayerName ?? Localizer["Console"];
        string message = string.Join(" ", args[1..]);

        SendMessageToReplyToCommand(info, false, "css_psay", adminname, targetname, message);
        SendMessageToPlayer(target, HudDestination.Chat, "css_psay", adminname, targetname, message);
    }
}