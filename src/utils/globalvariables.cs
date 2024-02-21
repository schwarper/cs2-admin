using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Commands.Targeting;
using CounterStrikeSharp.API.Modules.Entities.Constants;
using CounterStrikeSharp.API.Modules.Utils;
using MySqlConnector;

namespace Admin;

public partial class Admin : BasePlugin
{
    public static Admin Plugin { get; private set; } = new();
    public AdminConfig Config { get; set; } = new AdminConfig();

    private readonly List<Punishment> GlobalPunishList = new();
    private readonly Dictionary<string, int> GlobalVoteAnswers = new();
    private readonly List<CCSPlayerController> GlobalVotePlayers = new();
    private readonly Dictionary<CCSPlayerController, Vector> GlobalHRespawnPlayers = new();

    private bool GlobalVoteInProgress { get; set; } = false;
    private MySqlConnection GlobalDatabase = null!;

    private readonly string GlobalAdminsFilename = Server.GameDirectory + "/csgo/addons/counterstrikesharp/configs/admins.json";

    private readonly Dictionary<string, CsItem> GlobalWeaponDictionary = new()
    {
        { "zeus", CsItem.Taser },
        { "taser", CsItem.Taser },
        { "snowball", CsItem.Snowball },
        { "shield", CsItem.Shield },
        { "c4", CsItem.C4 },
        { "healthshot", CsItem.Healthshot },
        { "breachcharge", CsItem.BreachCharge },
        { "tablet", CsItem.Tablet },
        { "bumpmine", CsItem.Bumpmine },
        { "smoke", CsItem.SmokeGrenade },
        { "smokegrenade", CsItem.SmokeGrenade },
        { "flash", CsItem.Flashbang },
        { "flashbang", CsItem.Flashbang },
        { "hg", CsItem.HEGrenade },
        { "he", CsItem.HEGrenade },
        { "hegrenade", CsItem.HEGrenade },
        { "molotov", CsItem.Molotov },
        { "inc", CsItem.IncendiaryGrenade },
        { "incgrenade", CsItem.IncendiaryGrenade },
        { "decoy", CsItem.Decoy },
        { "ta", CsItem.TAGrenade },
        { "tagrenade", CsItem.TAGrenade },
        { "frag", CsItem.Frag },
        { "firebomb", CsItem.Firebomb },
        { "diversion", CsItem.Diversion },
        { "knife_t", CsItem.KnifeT },
        { "knife", CsItem.Knife },
        { "deagle", CsItem.Deagle },
        { "glock", CsItem.Glock },
        { "usp", CsItem.USPS },
        { "usp_silencer", CsItem.USPS },
        { "hkp2000", CsItem.HKP2000 },
        { "elite", CsItem.Elite },
        { "tec9", CsItem.Tec9 },
        { "p250", CsItem.P250 },
        { "cz75a", CsItem.CZ75 },
        { "fiveseven", CsItem.FiveSeven },
        { "revolver", CsItem.Revolver },
        { "mac10", CsItem.Mac10 },
        { "mp9", CsItem.MP9 },
        { "mp7", CsItem.MP7 },
        { "p90", CsItem.P90 },
        { "mp5", CsItem.MP5SD },
        { "mp5sd", CsItem.MP5SD },
        { "bizon", CsItem.Bizon },
        { "ump45", CsItem.UMP45 },
        { "xm1014", CsItem.XM1014 },
        { "nova", CsItem.Nova },
        { "mag7", CsItem.MAG7 },
        { "sawedoff", CsItem.SawedOff },
        { "m249", CsItem.M249 },
        { "negev", CsItem.Negev },
        { "ak", CsItem.AK47 },
        { "ak47", CsItem.AK47 },
        { "m4s", CsItem.M4A1S },
        { "m4a1s", CsItem.M4A1S },
        { "m4a1_silencer", CsItem.M4A1S },
        { "m4", CsItem.M4A1 },
        { "m4a1", CsItem.M4A1 },
        { "galil", CsItem.Galil },
        { "galilar", CsItem.Galil },
        { "famas", CsItem.Famas },
        { "sg556", CsItem.SG556 },
        { "awp", CsItem.AWP },
        { "aug", CsItem.AUG },
        { "ssg08", CsItem.SSG08 },
        { "scar20", CsItem.SCAR20 },
        { "g3sg1", CsItem.G3SG1 },
        { "kevlar", CsItem.Kevlar },
        { "assaultsuit", CsItem.AssaultSuit }
    };

    public enum MultipleFlags
    {
        NORMAL = 0,
        IGNORE_DEAD_PLAYERS,
        IGNORE_ALIVE_PLAYERS
    }

    private static readonly Dictionary<string, TargetType> TargetTypeMap = new(StringComparer.OrdinalIgnoreCase)
    {
        { "@all", TargetType.GroupAll },
        { "@bots", TargetType.GroupBots },
        { "@human", TargetType.GroupHumans },
        { "@alive", TargetType.GroupAlive },
        { "@dead", TargetType.GroupDead },
        { "@!me", TargetType.GroupNotMe },
        { "@me", TargetType.PlayerMe },
        { "@ct", TargetType.TeamCt },
        { "@t", TargetType.TeamT },
        { "@spec", TargetType.TeamSpec }
    };
}