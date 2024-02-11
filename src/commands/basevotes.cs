using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Modules.Menu;
using CounterStrikeSharp.API.Modules.Timers;
using System.Drawing;
using System.Text;
using System;

namespace Admin;
public partial class Admin : BasePlugin
{
    [ConsoleCommand("css_vote")]
    [RequiresPermissions("@css/generic")]
    [CommandHelper(minArgs: 2, "<question> [... Options ...]", whoCanExecute: CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Vote(CCSPlayerController? player, CommandInfo command)
    {
        if (command.ArgCount < 2)
        {
            return;
        }

        if(GlobalVoteInProgress)
        {
            command.ReplyToCommand(Localizer["Prefix"] + Localizer["css_vote<inprogress>"]);
            return;
        }

        GlobalVoteAnswers.Clear();

        string question = command.GetArg(1);
        int answersCount = command.ArgCount;

        CenterHtmlMenu menu = new(Localizer["css_vote<title>", question])
        {
            PostSelectAction = PostSelectAction.Nothing
        };

        string answer;
        StringBuilder arg = new ();

        for (int i = 2; i < answersCount; i++)
        {
            answer = command.GetArg(i);

            arg.Append($" <{command.GetArg(i)}>");

            GlobalVoteAnswers.Add(answer, 0);

            menu.AddMenuOption(Localizer["css_vote<optiontext>", answer, 0], HandleVote);
        }

        foreach (CCSPlayerController target in Utilities.GetPlayers().Where(p => p.Valid()))
        {
            MenuManager.OpenCenterHtmlMenu(GlobalBasePlugin, target, menu);
        }

        Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_vote", GetPlayerNameOrConsole(player), question]);

        _ = SendDiscordMessage($"[{GetPlayerSteamIdOrConsole(player)}] {GetPlayerNameOrConsole(player)} -> css_vote {arg}");

        GlobalVoteInProgress = true;

        AddTimer(30.0f, () =>
        {
            Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_vote<results>", question]);

            foreach (KeyValuePair<string, int> kvp in GlobalVoteAnswers)
            {
                Server.PrintToChatAll(Localizer["Prefix"] + Localizer["css_vote<resultsanswer>", kvp.Key, kvp.Value]);
            }

            GlobalVoteAnswers.Clear();
            GlobalVotePlayers.Clear();
            GlobalVoteInProgress = false;

            foreach (CCSPlayerController target in Utilities.GetPlayers().Where(p => p is { IsValid: true, IsBot: false }))
            {
                MenuManager.CloseActiveMenu(target);
            }

        }, TimerFlags.STOP_ON_MAPCHANGE);
    }

    public void HandleVote(CCSPlayerController player, ChatMenuOption option)
    {
        if (GlobalVoteInProgress && !GlobalVotePlayers.Contains(player))
        {
            string[] optiontexts = option.Text.Split(" ");

            GlobalVotePlayers.Add(player);
            GlobalVoteAnswers[optiontexts[0]]++;

            option.Text = Localizer["css_vote<optiontext>", optiontexts[0], GlobalVoteAnswers[optiontexts[0]]];
        }
    }
}