using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Commands;
using static Admin.FindTarget;
using static Admin.Library;

namespace Admin;

public partial class Admin
{
    public class PunishInfo
    {
        public ulong SteamID { get; set; }
        public string PunishName { get; set; } = string.Empty;
        public int Duration { get; set; }
        public DateTime Created { get; set; }
        public DateTime End { get; set; }
    }

    public static HashSet<ulong> PlayerGagList { get; set; } = [];
    public static List<PunishInfo> PlayerTemporaryPunishList { get; set; } = [];

    private static void OnBaseCommTimer()
    {
        DateTime now = DateTime.Now;

        List<PunishInfo> punishlist = PlayerTemporaryPunishList.FindAll(p => p.End < now);

        foreach (PunishInfo punish in punishlist)
        {
            if (punish.PunishName == "MUTE")
            {
                CCSPlayerController? player = Utilities.GetPlayerFromSteamId(punish.SteamID);

                if (player != null)
                {
                    player.VoiceFlags = VoiceFlags.Normal;
                }
            }
        }

        if (punishlist.Count > 0)
        {
            Task.Run(() => Database.RemoveExpiredPunishs());
        }

        PlayerTemporaryPunishList.RemoveAll(p => p.End < now);
    }

    [ConsoleCommand("css_mute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(1, "<#userid|name|all @ commands>", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Mute(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, false, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.VoiceFlags = VoiceFlags.Muted;
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_mute<player>", adminname, targetname);
        }
        else
        {
            PrintToChatAll("css_mute<multiple>", adminname, targetname);
        }

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_mute <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_unmute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(1, "<#userid|name|all @ commands>", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Unmute(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, false, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.VoiceFlags = VoiceFlags.Normal;
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_unmute<player>", adminname, targetname);
        }
        else
        {
            PrintToChatAll("css_unmute<multiple>", adminname, targetname);
        }

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_unmute <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_gag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(1, "<#userid|name|all @ commands>", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Gag(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, false, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            PlayerGagList.Add(target.SteamID);
            TagApi?.GagPlayer(target.SteamID);
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_gag<player>", adminname, targetname);
        }
        else
        {
            PrintToChatAll("css_gag<multiple>", adminname, targetname);
        }

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_gag <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_ungag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(1, "<#userid|name|all @ commands>", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Ungag(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, false, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            PlayerGagList.Remove(target.SteamID);
            TagApi?.UngagPlayer(target.SteamID);
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_ungag<player>", adminname, targetname);
        }
        else
        {
            PrintToChatAll("css_ungag<multiple>", adminname, targetname);
        }

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_ungag <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_silence")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(1, "<#userid|name|all @ commands>", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Silence(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, false, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.VoiceFlags = VoiceFlags.Muted;
            PlayerGagList.Add(target.SteamID);
            TagApi?.GagPlayer(target.SteamID);
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_silence<player>", adminname, targetname);
        }
        else
        {
            PrintToChatAll("css_silence<multiple>", adminname, targetname);
        }

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_silence <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_unsilence")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(1, "<#userid|name|all @ commands>", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_Unsilence(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, false, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        foreach (CCSPlayerController target in players)
        {
            target.VoiceFlags = VoiceFlags.Normal;
            PlayerGagList.Remove(target.SteamID);
            TagApi?.UngagPlayer(target.SteamID);
        }

        if (players.Count == 1)
        {
            PrintToChatAll("css_unsilence<player>", adminname, targetname);
        }
        else
        {
            PrintToChatAll("css_unsilence<multiple>", adminname, targetname);
        }

        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_unsilence <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_tmute")]
    [ConsoleCommand("css_smute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(1, "<#userid|name> <time>", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TMute(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, true, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        CCSPlayerController target = players.Single();

        if (PlayerTemporaryPunishList.Any(p => p.SteamID == target.SteamID && p.PunishName == "MUTE"))
        {
            command.ReplyToCommand(Config.Tag + Localizer["Already temp muted"]);
            return;
        }

        if (!int.TryParse(command.GetArg(2), out int time))
        {
            command.ReplyToCommand(Config.Tag + Localizer["Must be an integer"]);
            return;
        }

        DateTime now = DateTime.Now;

        target.VoiceFlags = VoiceFlags.Muted;

        PlayerTemporaryPunishList.Add(new PunishInfo
        {
            SteamID = target.SteamID,
            PunishName = "MUTE",
            Duration = time,
            Created = now,
            End = now.AddMinutes(time)
        });

        Task.Run(async () => await Database.PunishPlayer(target, targetname, player, adminname, "MUTE", time));

        PrintToChatAll("css_tmute", adminname, targetname, time);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_tmute <{command.GetArg(1)}> <{time}>");
    }

    [ConsoleCommand("css_tunmute")]
    [ConsoleCommand("css_sunmute")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(1, "<#userid|name>", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TUnmute(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, true, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        CCSPlayerController target = players.Single();

        if (!PlayerTemporaryPunishList.Any(p => p.SteamID == target.SteamID && p.PunishName == "MUTE"))
        {
            command.ReplyToCommand(Config.Tag + Localizer["Already not temp muted"]);
            return;
        }

        target.VoiceFlags = VoiceFlags.Normal;
        PlayerTemporaryPunishList.RemoveAll(p => p.SteamID == target.SteamID && p.PunishName == "MUTE");

        Task.Run(() => Database.UnPunishPlayer(target.SteamID, player, adminname, "MUTE"));

        PrintToChatAll("css_tunmute", adminname, targetname);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_tunmute <{command.GetArg(1)}> <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_tgag")]
    [ConsoleCommand("css_sgag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(1, "<#userid|name> <time>", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TGag(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, true, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        CCSPlayerController target = players.Single();

        if (PlayerTemporaryPunishList.Any(p => p.SteamID == target.SteamID && p.PunishName == "GAG"))
        {
            command.ReplyToCommand(Config.Tag + Localizer["Already temp gagged"]);
            return;
        }

        if (!int.TryParse(command.GetArg(2), out int time))
        {
            command.ReplyToCommand(Config.Tag + Localizer["Must be an integer"]);
            return;
        }

        DateTime now = DateTime.Now;

        PlayerTemporaryPunishList.Add(new PunishInfo
        {
            SteamID = target.SteamID,
            PunishName = "GAG",
            Duration = time,
            Created = now,
            End = now.AddMinutes(time)
        });

        Task.Run(() => Database.PunishPlayer(target, targetname, player, adminname, "GAG", time));

        PrintToChatAll("css_tgag", adminname, targetname, time);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_tgag <{command.GetArg(1)}> <{time}>");
    }

    [ConsoleCommand("css_tungag")]
    [ConsoleCommand("css_sungag")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(1, "<#userid|name>", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TUngag(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, true, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        CCSPlayerController target = players.Single();

        if (!PlayerTemporaryPunishList.Any(p => p.SteamID == target.SteamID && p.PunishName == "GAG"))
        {
            command.ReplyToCommand(Config.Tag + Localizer["Already not temp gagged"]);
            return;
        }

        PlayerTemporaryPunishList.RemoveAll(p => p.SteamID == target.SteamID && p.PunishName == "GAG");

        Task.Run(() => Database.UnPunishPlayer(target.SteamID, player, adminname, "GAG"));

        PrintToChatAll("css_tungag", adminname, targetname);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_tungag <{command.GetArg(1)}>");
    }

    [ConsoleCommand("css_tsilence")]
    [ConsoleCommand("css_ssilence")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(1, "<#userid|name> <time>", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TSilence(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, true, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        CCSPlayerController target = players.Single();

        if (!int.TryParse(command.GetArg(2), out int time))
        {
            command.ReplyToCommand(Config.Tag + Localizer["Must be an integer"]);
            return;
        }

        target.VoiceFlags = VoiceFlags.Muted;

        DateTime now = DateTime.Now;

        if (!PlayerTemporaryPunishList.Any(p => p.SteamID == target.SteamID && p.PunishName == "MUTE"))
        {
            PlayerTemporaryPunishList.Add(new PunishInfo
            {
                SteamID = target.SteamID,
                PunishName = "MUTE",
                Duration = time,
                Created = now,
                End = now.AddMinutes(time)
            });

            Task.Run(() => Database.PunishPlayer(target, targetname, player, adminname, "MUTE", time));
        }

        if (!PlayerTemporaryPunishList.Any(p => p.SteamID == target.SteamID && p.PunishName == "GAG"))
        {
            PlayerTemporaryPunishList.Add(new PunishInfo
            {
                SteamID = target.SteamID,
                PunishName = "GAG",
                Duration = time,
                Created = now,
                End = now.AddMinutes(time)
            });

            Task.Run(() => Database.PunishPlayer(target, targetname, player, adminname, "GAG", time));
        }

        PrintToChatAll("css_tsilence", adminname, targetname, time);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_tsilence <{command.GetArg(1)}> <{time}>");
    }

    [ConsoleCommand("css_tunsilence")]
    [ConsoleCommand("css_sunsilence")]
    [RequiresPermissions("@css/chat")]
    [CommandHelper(1, "<#userid|name>", CommandUsage.CLIENT_AND_SERVER)]
    public void Command_TUnSilence(CCSPlayerController? player, CommandInfo command)
    {
        (List<CCSPlayerController> players, string adminname, string targetname) = Find(player, command, 1, true, true, MultipleFlags.NORMAL);

        if (players.Count == 0)
        {
            return;
        }

        CCSPlayerController target = players.Single();

        PlayerTemporaryPunishList.RemoveAll(p => p.SteamID == target.SteamID && p.PunishName == "GAG");
        Task.Run(() => Database.UnPunishPlayer(target.SteamID, player, adminname, "GAG"));

        PlayerTemporaryPunishList.RemoveAll(p => p.SteamID == target.SteamID && p.PunishName == "MUTE");
        Task.Run(() => Database.UnPunishPlayer(target.SteamID, player, adminname, "MUTE"));

        PrintToChatAll("css_tunsilence", adminname, targetname);
        Discord.SendMessage($"[{player?.SteamID ?? 0}] {adminname} -> css_tunsilence <{command.GetArg(1)}>");
    }
}