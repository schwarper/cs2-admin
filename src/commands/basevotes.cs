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
    private bool GlobalVoteInProgress = false;
    private readonly Dictionary<string, int> GlobalVoteAnswers = [];
    private readonly HashSet<CCSPlayerController> GlobalVotePlayers = [];

    [ConsoleCommand("css_vote")]
    [RequiresPermissions("@css/generic")]
    [CommandHelper(minArgs: 2, "<question> [... Options ...]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Vote(CCSPlayerController? player, CommandInfo command)
    {
        if (GlobalVoteInProgress)
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["css_vote<inprogress>"]);
            return;
        }

        string question = command.GetArg(1);

        List<string> options = [];

        for (int i = 2; i < command.ArgCount; i++)
        {
            options.Add(command.GetArg(i));
        }

        GlobalVoteInProgress = true;
        GlobalVoteAnswers.Clear();
        GlobalVotePlayers.Clear();

        PrintToChatAll("css_vote", player?.PlayerName ?? "Console", question);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {player?.PlayerName ?? "Console"} -> css_vote {options[0]}");

        CenterHtmlMenu menu = VoteMenu(question, options);
        menu.OpenToAll();

        AddTimer(15.0f, () => EndVote(question), TimerFlags.STOP_ON_MAPCHANGE);
    }

    private CenterHtmlMenu VoteMenu(string question, List<string> options)
    {
        CenterHtmlMenu menu = new(Localizer["css_vote<title>", question], this)
        {
            PostSelectAction = PostSelectAction.Nothing
        };

        foreach (string option in options)
        {
            GlobalVoteAnswers.Add(option, 0);

            menu.AddMenuOption(Localizer["css_vote<optiontext>", option, 0], (p, o) =>
            {
                if (GlobalVoteInProgress && GlobalVotePlayers.Add(p))
                {
                    GlobalVoteAnswers[option]++;
                    o.Text = Localizer["css_vote<optiontext>", option, GlobalVoteAnswers[option]];
                }
            });
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
}