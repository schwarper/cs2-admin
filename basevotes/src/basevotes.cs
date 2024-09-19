using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core.Translations;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Utils;
using static BaseVotes.Library;
using Timer = CounterStrikeSharp.API.Modules.Timers.Timer;

namespace BaseVotes;

public class BaseVotes : BasePlugin, IPluginConfig<Config>
{
    public override string ModuleName => "Basic Votes";
    public override string ModuleVersion => "0.0.1";
    public override string ModuleAuthor => "schwarper";
    public override string ModuleDescription => "Basic Vote Commands";

    public static BaseVotes Instance { get; set; } = new BaseVotes();
    public Config Config { get; set; } = new Config();
    public Dictionary<CCSPlayerController, (string, ChatMenuOption)> GlobalVotePlayers { get; set; } = [];
    public CenterHtmlMenu? GlobalMenu { get; set; } = null;
    public bool GlobalVoteInProgress => GlobalMenu != null;
    private readonly Dictionary<string, int> GlobalVoteAnswers = [];
    private Timer? GlobalTimer;

    public override void Load(bool hotReload)
    {
        Instance = this;
    }

    public void OnConfigParsed(Config config)
    {
        config.Tag = StringExtensions.ReplaceColorTags(config.Tag);
        Config = config;
    }

    [ConsoleCommand("css_vote")]
    [RequiresPermissions("@css/generic")]
    [CommandHelper(minArgs: 2, usage: "<question> [... Options ...]")]
    public void Command_Vote(CCSPlayerController? player, CommandInfo info)
    {
        if (GlobalVoteInProgress)
        {
            SendMessageToReplyToCommand(info, "css_vote<inprogress>");
            return;
        }

        string[] args = info.ArgString.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        List<string> options = [];

        foreach (string option in args[1..])
        {
            options.Add(option);
        }

        ResetVote();

        string adminname = player?.PlayerName ?? Localizer["Console"];

        SendMessageToAllPlayers(HudDestination.Chat, "css_vote", adminname, args[0]);

        GlobalMenu = VoteMenu(args[0], options);
        GlobalMenu.OpenToAll();

        GlobalTimer = AddTimer(15.0f, () => EndVote(args[0]));
    }

    [ConsoleCommand("css_revote")]
    public void Command_Revote(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            return;
        }

        if (!GlobalVoteInProgress)
        {
            SendMessageToReplyToCommand(info, "Vote is not in progress");
            return;
        }

        if (!GlobalVotePlayers.TryGetValue(player, out (string, ChatMenuOption) value))
        {
            SendMessageToReplyToCommand(info, "You haven't voted yet");
            return;
        }

        GlobalVoteAnswers[value.Item1]--;

        ChatMenuOption? o = GlobalMenu?.MenuOptions.Find(o => o.Equals(GlobalVotePlayers[player].Item2));

        if (o != null)
        {
            string option = GlobalVotePlayers[player].Item1;

            o.Text = Localizer["css_vote<optiontext>", option, GlobalVoteAnswers[option]];
        }

        GlobalVotePlayers.Remove(player);

        SendMessageToReplyToCommand(info, "css_revote");
    }

    [ConsoleCommand("css_cancelvote")]
    [RequiresPermissions("@css/generic")]
    public void Command_Cancelvote(CCSPlayerController? player, CommandInfo info)
    {
        if (player == null)
        {
            return;
        }

        if (!GlobalVoteInProgress)
        {
            SendMessageToReplyToCommand(info, "Vote is not in progress");
            return;
        }

        ResetVote();

        foreach (CCSPlayerController target in Utilities.GetPlayers())
        {
            MenuManager.CloseActiveMenu(target);
        }

        SendMessageToAllPlayers(HudDestination.Chat, "css_cancelvote", player.PlayerName);
    }

    private CenterHtmlMenu VoteMenu(string question, List<string> options)
    {
        CenterHtmlMenu menu = new(Localizer["css_vote<title>", question], this)
        {
            PostSelectAction = PostSelectAction.Nothing
        };

        foreach (string option in options)
        {
            try
            {
                GlobalVoteAnswers.Add(option, 0);

                menu.AddMenuOption(Localizer["css_vote<optiontext>", option, 0], (p, o) =>
                {
                    if (GlobalVoteInProgress && !GlobalVotePlayers.ContainsKey(p))
                    {
                        GlobalVotePlayers.Add(p, (option, o));
                        GlobalVoteAnswers[option]++;

                        o.Text = Localizer["css_vote<optiontext>", option, GlobalVoteAnswers[option]];
                    }
                });
            }
            catch (Exception) { };
        }

        return menu;
    }

    private void EndVote(string question)
    {
        SendMessageToAllPlayers(HudDestination.Chat, "css_vote<results>", question);

        foreach (KeyValuePair<string, int> kvp in GlobalVoteAnswers)
        {
            SendMessageToAllPlayers(HudDestination.Chat, "css_vote<resultsanswer>", kvp.Key, kvp.Value);
        }

        GlobalVoteAnswers.Clear();
        GlobalVotePlayers.Clear();
        GlobalMenu = null;

        foreach (CCSPlayerController target in Utilities.GetPlayers())
        {
            MenuManager.CloseActiveMenu(target);
        }
    }

    private void ResetVote()
    {
        GlobalVoteAnswers.Clear();
        GlobalVotePlayers.Clear();
        GlobalTimer?.Kill();
        GlobalMenu = null;
    }
}