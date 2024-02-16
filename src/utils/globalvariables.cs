using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Modules.Utils;
using MySqlConnector;

namespace Admin;

public partial class Admin : BasePlugin
{
    public required Admin GlobalBasePlugin;
    public AdminConfig Config { get; set; } = new AdminConfig();

    private readonly List<Punishment> GlobalPunishList = new();
    private readonly Dictionary<string, int> GlobalVoteAnswers = new();
    private readonly List<CCSPlayerController> GlobalVotePlayers = new();
    private readonly Dictionary<CCSPlayerController, Vector> GlobalHRespawnPlayers = new();

    private bool GlobalVoteInProgress { get; set; } = false;
    private MySqlConnection GlobalDatabase = null!;

    private readonly string GlobalAdminsFilename = Server.GameDirectory + "/csgo/addons/counterstrikesharp/configs/admins.json";

    private readonly string[] GlobalWeaponAllList =
    {
        "taser", "snowball", "shield", "c4", "healthshot", "breachcharge", "tablet", "bumpmine",
        "smokegrenade", "flashbang", "hegrenade", "molotov", "incgrenade", "decoy", "tagrenade",
        "frag", "firebomb", "diversion", "knife_t", "knife",
        "deagle", "glock", "usp_silencer", "hkp2000", "elite", "tec9", "p250", "cz75a", "fiveseven", "revolver",
        "mac10", "mp9", "mp7", "p90", "mp5sd", "bizon", "ump45", "xm1014", "nova", "mag7", "sawedoff", "m249", "negev",
        "ak47", "m4a1_silencer", "m4a1", "galilar", "famas", "sg556", "awp", "aug", "ssg08", "scar20", "g3sg1"
    };

    enum MultipleFlags
    {
        NORMAL = 0,
        IGNORE_DEAD_PLAYERS,
        IGNORE_ALIVE_PLAYERS
    }
}