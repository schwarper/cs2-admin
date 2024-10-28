using CounterStrikeSharp.API.Modules.Admin;
using CounterStrikeSharp.API.Modules.Entities;
using System.Reflection;

namespace BaseAdminSql;

public static class AdminManagerEx
{
    public static void AddAdmin(ulong steamid, HashSet<string> flags, HashSet<string> groups, uint immunity)
    {
        Dictionary<SteamID, AdminData> CoreAdmins = GetAdminManagerCore<Dictionary<SteamID, AdminData>>("Admins");

        AdminData admin = CoreAdmins[new SteamID(steamid)] = new AdminData()
        {
            Identity = steamid.ToString(),
            _flags = flags,
            Groups = groups,
            Immunity = immunity
        };

        admin.InitalizeFlags();
    }

    public static void RemoveAdmin(ulong steamid)
    {
        Dictionary<SteamID, AdminData> CoreAdmins = GetAdminManagerCore<Dictionary<SteamID, AdminData>>("Admins");

        CoreAdmins.Remove(new SteamID(steamid));
    }

    public static void AddGroup(string group, HashSet<string> flags, uint immunity)
    {
        Dictionary<string, AdminGroupData> CoreGroups = GetAdminManagerCore<Dictionary<string, AdminGroupData>>("Groups");

        CoreGroups[group] = new AdminGroupData()
        {
            Flags = flags,
            Immunity = immunity
        };
    }

    public static void RemoveGroup(string group)
    {
        Dictionary<string, AdminGroupData> CoreGroups = GetAdminManagerCore<Dictionary<string, AdminGroupData>>("Groups");

        CoreGroups.Remove(group);
    }

    public static bool IsAdminExist(ulong steamid)
    {
        Dictionary<SteamID, AdminData> CoreAdmins = GetAdminManagerCore<Dictionary<SteamID, AdminData>>("Admins");

        return CoreAdmins.ContainsKey(new SteamID(steamid));
    }

    public static bool IsGroupExist(string group)
    {
        Dictionary<string, AdminGroupData> CoreGroups = GetAdminManagerCore<Dictionary<string, AdminGroupData>>("Groups");

        return CoreGroups.ContainsKey(group);
    }

    public static T GetAdminManagerCore<T>(string field) where T : class
    {
        T? core = typeof(AdminManager).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static)?.GetValue(null) as T;
        return core ?? Activator.CreateInstance<T>();
    }
}