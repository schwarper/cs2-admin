using CounterStrikeSharp.API;
using CounterStrikeSharp.API.Core;
using CounterStrikeSharp.API.Core.Attributes.Registration;
using CounterStrikeSharp.API.Modules.Cvars;
using CounterStrikeSharp.API.ValveConstants.Protobuf;
using System.Collections.Concurrent;
using static CounterStrikeSharp.API.Core.Listeners;
using static CounterStrikeSharp.API.Modules.Admin.AdminManager;

namespace ReservedSlots;

public class ReservedSlots : BasePlugin
{
    public override string ModuleName => "Reserved Slots";
    public override string ModuleVersion => "1.5";
    public override string ModuleAuthor => "schwarper";
    public override string ModuleDescription => "Provides basic reserved slots";

    public FakeConVar<int> css_reserved_slots = new("css_reserved_slots", "Number of reserved player slots", 0);
    public FakeConVar<bool> css_hide_slots = new("css_hide_slots", "If set to 1, reserved slots will be hidden (subtracted from the max slot count)", false);
    public FakeConVar<int> css_reserve_type = new("css_reserve_type", "Method of reserving slots", 0);
    public FakeConVar<int> css_reserve_maxadmins = new("css_reserve_maxadmins", "Maximum amount of admins to let in the server with reserve type 2", 0);
    public FakeConVar<int> css_reserve_kicktype = new("css_reserve_kicktype", "How to select a client to kick (if appropriate)", 0);
    public ConVar sv_visiblemaxplayers = null!;
    private const string playerdesignername = "cs_player_controller";

    public ConcurrentDictionary<CCSPlayerController, double> GlobalPlayerTime = [];

    private enum KickType
    {
        Kick_HighestPing = 0,
        Kick_HighestTime,
        Kick_Random,
    };

    public List<CCSPlayerController> AdminsList { get; set; } = [];

    public override void Load(bool hotReload)
    {
        if (hotReload)
        {
            CheckHiddenSlots();
        }

        SlotCountChanged();
        RegisterListener<OnMapStart>(OnMapStart);

        sv_visiblemaxplayers = ConVar.Find("sv_visiblemaxplayers")!;
    }

    public override void Unload(bool hotReload)
    {
        RemoveListener<OnMapStart>(OnMapStart);

        ResetVisibleMax();
    }

    public void OnMapStart(string mapname)
    {
        CheckHiddenSlots();
    }

    public void OnTimedKick(CCSPlayerController player)
    {
        if (!player.IsValid)
        {
            return;
        }

        player.Disconnect(NetworkDisconnectionReason.NETWORK_DISCONNECT_REJECT_RESERVED_FOR_LOBBY);

        CheckHiddenSlots();
    }

    [GameEventHandler]
    public HookResult OnPlayerConnect(EventPlayerConnectFull @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        GlobalPlayerTime.TryAdd(player, Server.CurrentTime);

        int reserved = css_reserved_slots.Value;

        if (reserved > 0)
        {
            int clients = GetClientCount();
            int limit = Server.MaxPlayers - reserved;

            int type = css_reserve_type.Value;

            if (type == 0)
            {
                if (clients <= limit || player.IsBot || PlayerHasPermissions(player, "@css/reservation"))
                {
                    if (css_hide_slots.Value)
                    {
                        SetVisibleMaxSlots(clients, limit);
                    }

                    return HookResult.Continue;
                }

                AddTimer(0.1f, () => OnTimedKick(player));
            }
            else if (type == 1)
            {
                if (clients > limit)
                {
                    if (PlayerHasPermissions(player, "@css/reservation"))
                    {
                        CCSPlayerController? target = SelectKickClient();

                        if (target != null)
                        {
                            AddTimer(0.1f, () => OnTimedKick(target));
                        }
                    }
                    else
                    {
                        AddTimer(0.1f, () => OnTimedKick(player));
                    }
                }
            }
            else if (type == 2)
            {
                if (PlayerHasPermissions(player, "@css/reservation"))
                {
                    AdminsList.Add(player);
                }

                if (clients > limit && AdminsList.Count < css_reserve_maxadmins.Value)
                {
                    if (AdminsList.Contains(player))
                    {
                        CCSPlayerController? target = SelectKickClient();

                        if (target != null)
                        {
                            AddTimer(0.1f, () => OnTimedKick(target));
                        }
                    }
                    else
                    {
                        AddTimer(0.1f, () => OnTimedKick(player));
                    }
                }
            }
        }

        return HookResult.Continue;
    }

    [GameEventHandler]
    public HookResult OnClientDisconnect(EventPlayerDisconnect @event, GameEventInfo info)
    {
        CCSPlayerController? player = @event.Userid;

        if (player == null)
        {
            return HookResult.Continue;
        }

        CheckHiddenSlots();
        AdminsList.Remove(player);
        GlobalPlayerTime.TryRemove(player, out _);

        return HookResult.Continue;
    }

    public void SlotCountChanged()
    {
        css_reserved_slots.ValueChanged += (_, value) =>
        {
            if (value <= 0)
            {
                ResetVisibleMax();
            }
            else if (css_hide_slots.Value)
            {
                SetVisibleMaxSlots(GetClientCount(), Server.MaxPlayers - value);
            }
        };

        css_hide_slots.ValueChanged += (_, value) =>
        {
            if (!value)
            {
                ResetVisibleMax();
            }
            else
            {
                SetVisibleMaxSlots(GetClientCount(), Server.MaxPlayers - (value ? 1 : 0));
            }
        };
    }

    public void CheckHiddenSlots()
    {
        if (css_hide_slots.Value)
        {
            SetVisibleMaxSlots(GetClientCount(), Server.MaxPlayers - (css_hide_slots.Value ? 1 : 0));
        }
    }

    public void SetVisibleMaxSlots(int clients, int limit)
    {
        int num = clients;

        if (clients == Server.MaxPlayers)
        {
            num = Server.MaxPlayers;
        }
        else if (clients < limit)
        {
            num = limit;
        }

        sv_visiblemaxplayers.SetValue(num);
    }

    public void ResetVisibleMax()
    {
        sv_visiblemaxplayers.SetValue(-1);
    }

    public CCSPlayerController? SelectKickClient()
    {
        KickType type = (KickType)css_reserve_kicktype.Value;

        float highestValue = 0;
        CCSPlayerController? highestValuePlayer = null;

        float highestSpecValue = 0;
        CCSPlayerController? highestSpecValuePlayer = null;

        bool specFound = false;

        float value;

        for (int i = 0; i < Server.MaxPlayers; i++)
        {
            CCSPlayerController? player = Utilities.GetEntityFromIndex<CCSPlayerController>(i + 1);

            if (player?.IsValid is not true || player.IsBot || player.DesignerName != playerdesignername)
            {
                continue;
            }

            if (PlayerHasPermissions(player, "@css/reservation"))
            {
                continue;
            }

            value = 0.0f;

            if (player.Connected == PlayerConnectedState.PlayerConnected)
            {
                if (type == KickType.Kick_HighestPing)
                {
                    value = player.Ping;
                }
                else if (type == KickType.Kick_HighestTime)
                {
                    value = (float)-GlobalPlayerTime[player];
                }
                else
                {
                    value = Random.Shared.Next(0, 100);
                }

                if (player.ObserverPawn.Value != null)
                {
                    specFound = true;

                    if (value > highestSpecValue)
                    {
                        highestSpecValue = value;
                        highestSpecValuePlayer = player;
                    }
                }
            }

            if (value >= highestValue)
            {
                highestValue = value;
                highestValuePlayer = player;
            }
        }

        if (specFound)
        {
            return highestSpecValuePlayer;
        }

        return highestValuePlayer;
    }

    public static int GetClientCount()
    {
        int count = 0;

        for (int i = 0; i < Server.MaxPlayers; i++)
        {
            CCSPlayerController? player = Utilities.GetEntityFromIndex<CCSPlayerController>(i + 1);

            if (player?.IsValid is not true || player.IsBot || player.DesignerName != playerdesignername)
            {
                continue;
            }

            count++;
        }

        return count;
    }
}