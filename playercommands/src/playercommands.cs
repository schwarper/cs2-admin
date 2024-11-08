using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Utils;
using static PlayerCommands.Library;

namespace PlayerCommands;

public class PlayerCommands : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Player Commands";
    public override string ModuleVersion => "1.7";
    public override string ModuleAuthor => "schwarper";
    public override string ModuleDescription => "Misc. Player Commands";

    public static PlayerCommands Instance { get; set; } = new PlayerCommands();
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

    [ConsoleCommand("css_slap")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, usage: "<#userid|name|all @ commands> <damage>")]
    public void Command_Slap(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
        {
            return;
        }

        if (args.Length < 2 || !int.TryParse(args[1], out int damage))
        {
            damage = 0;
        }

        foreach (CBasePlayerPawn? targetPawn in players.Select(p => p.Pawn.Value))
        {
            if (targetPawn == null)
            {
                continue;
            }

            targetPawn.Slap(damage);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_slap<player>", adminname, targetname, damage);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_slap<multiple>", adminname, targetname, damage);
        }
    }

    [ConsoleCommand("css_slay")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 1, usage: "<#userid|name|all @ commands>")]
    public void Command_Slay(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], false, true, MultipleFlags.IGNORE_DEAD_PLAYERS, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
        {
            return;
        }

        foreach (CCSPlayerPawn? targetPlayerPawn in players.Select(p => p.PlayerPawn.Value))
        {
            if (targetPlayerPawn == null)
            {
                continue;
            }

            targetPlayerPawn.TakesDamage = true;
            targetPlayerPawn.CommitSuicide(false, true);
        }

        if (players.Count == 1)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_slay<player>", adminname, targetname);
        }
        else
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_slay<player>", adminname, targetname);
        }
    }

    [ConsoleCommand("css_rename")]
    [RequiresPermissions("@css/slay")]
    [CommandHelper(minArgs: 2, usage: "<#userid|name> <newname>")]
    public void Command_ReName(CCSPlayerController? player, CommandInfo info)
    {
        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (!ProcessTargetString(player, info, args[0], true, true, MultipleFlags.NORMAL, out List<CCSPlayerController>? players, out string? adminname, out string? targetname))
        {
            return;
        }

        CCSPlayerController target = players[0];

        string newname = string.Join(" ", args[1..]);

        if (string.IsNullOrEmpty(newname))
        {
            SendMessageToReplyToCommand(info, "Must be a string");
            return;
        }

        SendMessageToAllPlayers(HudDestination.Chat, "css_rename", adminname, targetname, newname);

        target.Rename(newname);
    }
}