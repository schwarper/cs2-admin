using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using static Admin.Library;

namespace Admin;

public partial class Admin
{
    private readonly Dictionary<CCSPlayerController, (string, ChatMenuOption)> GlobalVotePlayers = [];
    private CenterHtmlMenu GlobalMenu = null!;
    private bool GlobalVoteInProgress = false;
    private readonly Dictionary<string, int> GlobalVoteAnswers = [];

    //private readonly Dictionary<CCSPlayerController, string> GlobalVotePlayers = [];

    [ConsoleCommand("css_vote")]
    [RequiresPermissions("@css/generic")]
    [CommandHelper(minArgs: 2, "<question> [... Options ...]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Vote(CCSPlayerController? player, CommandInfo command)
    {
        if (GlobalVoteInProgress)
        {
            command.ReplyToCommand(Config.Tag + Localizer["css_vote<inprogress>"]);
            return;
        }

        string question = command.GetArg(1);

        List<string> options = [];

        for (int i = 2; i < command.ArgCount; i++)
        {
            options.Add(command.GetArg(i));
        }

        ResetVote();

        PrintToChatAll("css_vote", player?.PlayerName ?? "Console", question);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {player?.PlayerName ?? "Console"} -> css_vote {options[0]}");

        CenterHtmlMenu menu = GlobalMenu = VoteMenu(question, options);
        menu.OpenToAll();

        GlobalVoteInProgress = true;

        AddTimer(15.0f, () => EndVote(question), TimerFlags.STOP_ON_MAPCHANGE);
    }

    [ConsoleCommand("css_revote")]
    public void Command_Revote(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            return;
        }

        if (!GlobalVoteInProgress)
        {
            command.ReplyToCommand(Config.Tag + Localizer["Vote is not in progress"]);
            return;
        }

        if (!GlobalVotePlayers.TryGetValue(player, out (string, ChatMenuOption) value))
        {
            command.ReplyToCommand(Config.Tag + Localizer["You haven't voted yet"]);
            return;
        }

        GlobalVoteAnswers[value.Item1]--;

        ChatMenuOption? o = GlobalMenu.MenuOptions.Find(o => o.Equals(GlobalVotePlayers[player].Item2));

        if (o != null)
        {
            string option = GlobalVotePlayers[player].Item1;

            o.Text = Localizer["css_vote<optiontext>", option, GlobalVoteAnswers[option]];
        }

        GlobalVotePlayers.Remove(player);

        command.ReplyToCommand(Config.Tag + Localizer["css_revote"]);
    }

    [ConsoleCommand("css_cancelvote")]
    [RequiresPermissions("@css/generic")]
    public void Command_Cancelvote(CCSPlayerController? player, CommandInfo command)
    {
        if (player == null)
        {
            return;
        }

        if (!GlobalVoteInProgress)
        {
            command.ReplyToCommand(Config.Tag + Localizer["Vote is not in progress"]);
            return;
        }

        ResetVote();

        foreach (CCSPlayerController target in Utilities.GetPlayers())
        {
            MenuManager.CloseActiveMenu(target);
        }

        PrintToChatAll("css_cancelvote", player.PlayerName);
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
        GlobalVoteInProgress = false;

        PrintToChatAll("css_vote<results>", question);

        foreach (KeyValuePair<string, int> kvp in GlobalVoteAnswers)
        {
            PrintToChatAll("css_vote<resultsanswer>", kvp.Key, kvp.Value);
        }

        GlobalVoteAnswers.Clear();
        GlobalVotePlayers.Clear();

        foreach (CCSPlayerController target in Utilities.GetPlayers())
        {
            MenuManager.CloseActiveMenu(target);
        }
    }

    private void ResetVote()
    {
        GlobalVoteInProgress = false;
        GlobalVoteAnswers.Clear();
        GlobalVotePlayers.Clear();
    }
}