## CS2-ADMIN

If you want to donate or need a help about plugin, you can contact me in discord private/server

Discord nickname: schwarper

Discord link : [Discord server](https://discord.gg/4zQfUzjk36)

# Info
Remake of [the admin plugins made by Alliedmodders for CSGO](https://github.com/alliedmodders/sourcemod/tree/master/plugins).

# Plugins
- [Anti-Flood](#anti-flood)
- [Basic Admin](#base-admin)
- [Basic Ban Commands](#basic-ban-commands)
- [Basic Chat](#basic-chat)
- [Basic Comm Control](#basic-comm-control)
- [Basic Commands](#basic-commands)
- [Basic Votes](#basic-votes)
- [Fun Commands](#fun-commands)
- [Player Commands](#player-commands)
- [Reserved Slots](#reserved-slots)


# Anti-Flood
```js
css_flood_duration 0.75 - Amount of duration allowed between chat messages
```

# Base Admin
```js
♦ css_addadmin <steamid> <group> <immunity> [ Default perm is @css/root ]
♦ css_removeadmin <steamid> [ Default perm is @css/root ]
♦ css_addgroup <group> <flags> <immunity> [ Default perm is @css/root ]
♦ css_removegroup <group>  [ Default perm is @css/root ]

Usage:
css_addgroup GroupName ban,unban,kick 10: Adds #GroupName and gives @css/ban, @css/unban, @css/kick permissions and 10 immunity level.
css_addgroup GroupName2 ban,unban,kick: Adds #GroupName2 as a group and gives @css/ban, @css/unban, @css/kick permissions.
css_removegroup GroupName2: Removes #GroupName2 group.

css_addadmin 76561199165718810 GroupName 10: Gives #GroupName to 76561199165718810 SteamID and 10 immunity level.
css_addadmin 76561199165718810 GroupName2: Gives #GroupName2 to 76561199165718810 SteamID.
css_removeadmin 76561199165718810: Removes 76561199165718810 SteamID.
```

# Basic Ban Commands
```js
♦ css_ban <#userid|name> <minutes|0> [reason] [ Default perm is @css/ban ]
♦ css_unban <steamid> [ Default perm is @css/unban ]
♦ css_addban <duration> <steamid> [reason] [ Default perm is @css/root ]
```

# Basic Chat
```js
css_chat_mode false - enables say_team @. Default is false.

♦ say_team @ text - sends message to admins [ Everyone can use this ]
♦ css_say <message> - sends message to all players [ Default perm is @css/chat ]
♦ css_csay <message> - sends centered message to all players [ Default perm is @css/chat ]
♦ css_hsay <message> - <message> - sends hud message to all players [ Default perm is @css/chat ]
♦ css_asay <message> - <message> - sends message to admins [ Default perm is @css/chat ]
♦ css_chat <message> - <message> - sends message to admins [ Default perm is @css/chat ]
♦ css_psay <#userid|name> <message> - sends public message [ Default perm is @css/chat ]
```

# Basic Comm Control
```js
♦ css_mute <#userid|name|all @ commands> - Removes a player's ability to use voice. [ Default perm is @css/chat ]
♦ css_unmute <#userid|name|all @ commands> - Restores a player's ability to use voice. [ Default perm is @css/chat ]
♦ css_gag <#userid|name|all @ commands> - Removes a player's ability to use chat. [ Default perm is @css/chat ]
♦ css_ungag <#userid|name|all @ commands> - Restores a player's ability to use chat. [ Default perm is @css/chat ]
♦ css_silence <#userid|name|all @ commands> - Removes a player's ability to use voice or chat. [ Default perm is @css/chat ]
♦ css_unsilence <#userid|name|all @ commands> - Restores a player's ability to use voice and chat. [ Default perm is @css/chat ]
```

# Basic Commands
```js
♦ css_kick <#userid|name> [reason] [ Default perm is @css/kick ]
♦ css_changemap <map> [ Default perm is @css/map ]
♦ css_map <map> [ Default perm is @css/map ]
♦ css_workshop <map> [ Default perm is @css/map ]
♦ css_wsmap <map> [ Default perm is @css/map ]
♦ css_rcon <args> [ Default perm is @css/rcon ]
♦ css_cvar <cvar> <value> [ Default perm is @css/cvar ]
♦ css_exec <exec> [ Default perm is @css/config ]
♦ css_who <#userid|name or empty for all> [ Default perm is @css/generic ]
```

# Basic Temp Comm Control
```js
♦ css_smute <#userid|name> <time> <reason> - Imposes a timed mute [ Default perm is @css/chat ]
♦ css_tmute <#userid|name> <time> <reason> - Imposes a timed mute [ Default perm is @css/chat ]
♦ css_sunmute <#userid|name> - Unmute timed mute [ Default perm is @css/chat ]
♦ css_tunmute <#userid|name> - Unmute timed mute [ Default perm is @css/chat ]
♦ css_gag <#userid|name> <time> - Imposes a timed gag [ Default perm is @css/chat ]
♦ css_gag <#userid|name> <time> - Imposes a timed gag [ Default perm is @css/chat ]
♦ css_ungag <#userid|name> - Ungag timed gag [ Default perm is @css/chat ]
♦ css_ungag <#userid|name> - Ungag timed gag [ Default perm is @css/chat ]
```

# Basic Votes
```js
♦ css_vote <question> [... Options ...] [ Default perm is @css/generic ]
♦ css_revote
♦ css_cancelvote [ Default perm is @css/generic ]
```

# Fun Commands
```js
♦ css_freeze <#userid|name|all @ commands> <time> [ Default perm is @css/slay ]
♦ css_unfreeze <#userid|name|all @ commands> [ Default perm is @css/slay ]
♦ css_gravity <gravity> [ Default perm is @css/slay ]
♦ css_revive <#userid|name|all @ commands> [ Default perm is @css/cheats ]
♦ css_respawn <#userid|name|all @ commands> [ Default perm is @css/cheats ]
♦ css_noclip <#userid|name|all @ commands> <value> [ Default perm is @css/cheats ]
♦ css_weapon <#userid|name|all @ commands> <weapon> [ Default perm is @css/cheats ]
♦ css_strip <#userid|name|all @ commands> [ Default perm is @css/cheats ]
♦ css_sethp <team> <health> - Sets team players' spawn health [ Default perm is @css/cheats ]
♦ css_hp <#userid|name|all @ commands> <health> [ Default perm is @css/cheats ]
♦ css_speed <#userid|name|all @ commands> <value> [ Default perm is @css/cheats ]
♦ css_god <#userid|name|all @ commands> <value> [ Default perm is @css/cheats ]
♦ css_team <#userid|name|all @ commands> <value> [ Default perm is @css/kick ]
♦ css_swap <#userid|name> [ Default perm is @css/kick ]
♦ css_bury <#userid|name|all @ commands> [ Default perm is @css/slay ]
♦ css_unbury <#userid|name|all @ commands> [ Default perm is @css/slay ]
♦ css_clean - Clean weapons on the ground [ Default perm is @css/slay ]
♦ css_goto - <#userid|name> - Teleport player to a player's position [ Default perm is @css/slay ]
♦ css_bring - <#userid|name|all @ commands> - Teleport players to a player's position [ Default perm is @css/slay ]
♦ css_hrespawn - <#userid|name> - Respawns a player in his last known death position. [ Default perm is @css/slay ]
♦ css_1up - <#userid|name> - Respawns a player in his last known death position. [ Default perm is @css/slay ]
♦ css_glow <#userid|name|all @ commands> <color> [ Default perm is @css/slay ]
♦ css_color <#userid|name|all @ commands> <color> [ Default perm is @css/slay ]
♦ css_beacon <#userid|name|all @ commands> <value> [ Default perm is @css/slay ]
♦ css_shake <#userid|name|all @ commands> <time> [ Default perm is @css/slay ]
♦ css_unshake <#userid|name|all @ commands> [ Default perm is @css/slay ]
♦ css_blind <#userid|name|all @ commands> <time> [ Default perm is @css/slay ]
♦ css_unblind <#userid|name|all @ commands> [ Default perm is @css/slay ]
```

# Player Commands
```js
♦ css_slap <#userid|name|all @ commands> <damage> [ Default perm is @css/slay ]
♦ css_slay <#userid|name|all @ commands> [ Default perm is @css/slay ]
♦ css_rename <#userid|name> <newname> [ Default perm is @css/slay ]
```

# Reserved Slots
[Reserved Slots (SourceMod)](https://wiki.alliedmods.net/Reserved_Slots_(SourceMod))
```js
css_reserved_slots <#>
/*
   - This controls how many slots get reserved by the plugin (the default is 0).
   - Using css_reserve_type 0 this is how many admins can join the server after it appears full to the public. Using css_reserve_type 1 this is how many slots are saved for swapping admins in (you shouldn't need more than one)
*/
css_hide_slots <0|1>
/*
  - This controls the plugin hides the reserved slots (the default is 0).
  - If enabled (1) reserve slots are hidden in the server browser window when they are not in use. For example a 24 player server with 2 reserved slots will show as a 22 player server (until the reserved slots are occupied). If you experience that the slots are not hidden, despite setting css_hide_slots to 1, then adding host_info_show 2 to your server.cfg may solve this problem. To connect to the reserved slot of a server that shows as full you will need to use 'connect ip:port' in console. (e.g. 'connect 192.168.1.100:27015').
  - There is no possible way for the reserved slots to be visible to admins and hidden from normal users. Admin authentication can only happen after the user is fully connected to the server and their steam id is available to SourceMod. For this reason it is often better to hide the slots otherwise public users will attempt to join the server and will get kicked again (rendering the ‘autojoin’ feature useless)
*/
css_reserve_type <0|1|2>
/*
  - This controls how reserve slots work on the server (the default is 0).

  - css_reserve_type 0
  - Public slots are used in preference to reserved slots. Reserved slots are freed before public slots. No players are ever kicked and once reserved slots are filled by a reserve slot player (and the rest of the server is full) they will remain occupied until a player leaves. The use of this is that there can always be at least one admin (assuming you only give reserved slots to admins) on the server at any time. If players inform you that there is a cheater on the server, at least one admin should be able to get it and do something about it. If a player without reserve slot access joins when there are only reserved spaces remaining they will be kicked from the server.

  - css_reserve_type 1
  - If someone with reserve access joins into a reserved slot, the player with the highest latency and without reserve access (spectator players are selected first) is kicked to make room. Thus, the reserved slots always remain free. The only situation where the reserved slot(s) can become properly occupied is if the server is full with reserve slot access clients. This is for servers that want some people to have playing preference over other. With this method admins could one by one join a full server until they all get in.

  - css_reserve_type 2 - Only available in SourceMod 1.1 or higher.
  - The same as css_reserve_type 1 except once a certain number of admins have been reached the reserve slot stops kicking people and anyone can join to fill the server. You can use this to simulate having a large number of reserved slots with css_reserve_type 0 but with only need to have 1 slot unavailable when there are less admins connected.
*/
css_reserve_maxadmins <#>
/*
  - This controls how many admins can join the server before the reserved slots are made public (only relevant to css_reserve_type 2)
*/
css_reserve_kicktype <0|1|2>
/*
  - This controls how a client is selected to be kicked (only relevant to css_reserve_type 1/2)

  - 0 - Highest Ping
  - 1 - Highest Connection Time
  - 2 - Random Player
*/
