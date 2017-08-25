# Colony Survival SpawnProtect Mod by Scarabol

This mods adds spawn protection and banner protection to your server. Griefers can not add or remove any blocks in a configurable area around your spawn. Furthermore a player can not build in the range of another players banner.

## Configuration

The mod directory contains a json file (**protection-ranges.json**) to configure the mod protection ranges on x and z axis. The y axis (height/depth) is protected from bedrock to the sky.

## Permissions

*mods.scarabol.spawnprotect*

* super-permission, grants **all** permissions in this mod

*mods.scarabol.spawnprotect.spawnchange*

* grants permission to change blocks in the spawn range
* you can use */spawnprotect spawn [steamid]* to grant this permission to a player, **which has played on this server before**
* to revoke the permission, enter */spawnprotect nospawn [steamid]*

*mods.scarabol.spawnprotect.banner.[steamid]*

* grants permission for its owner, to change the blocks in the banner range of another player [steamid]
* **everybody** can grant and revoke permissions for personal banner range, with */spawnprotect grant [steamid]* and */spawnprotect deny [steamid]*

For example you want to grant permission to your friend (Steam-ID: 123456789) to change blocks in your banner range. Just enter **/spawnprotect grant 123456789** in the chat. Then ask your friend to do the same with your Steam-ID...

## Installation

**This mod must be installed on the server side!** No client installation required.

* download a (compatible) [release](https://github.com/Scarabol/ColonySpawnProtectMod/releases) or build from source code (see below)
* place the unzipped *Scarabol* folder inside your *ColonySurvival/gamedata/mods/* directory, like *ColonySurvival/gamedata/mods/Scarabol/*

## Build

* install Linux
* download source code
```Shell
git clone https://github.com/Scarabol/ColonySpawnProtectMod
```
* use make
```Shell
cd ColonyConstructionMod
make
```

**Pull requests welcome!**

