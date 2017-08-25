using System;
using System.IO;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pipliz;
using Pipliz.Chatting;
using Pipliz.JSON;
using Pipliz.Threading;
using Permissions;
using NPC;

namespace ScarabolMods
{
  [ModLoader.ModManager]
  public static class SpawnProtectModEntries
  {
    public static string PERMISSION_SUPER = "mods.scarabol.spawnprotect";
    public static string PERMISSION_SPAWN_CHANGE = PERMISSION_SUPER + ".spawnchange";
    public static string PERMISSION_BANNER_PREFIX = PERMISSION_SUPER + ".banner.";
    private static int SpawnProtectionRangeX = 50;
    private static int SpawnProtectionRangeZ = 50;
    private static int BannerProtectionRangeX = 50;
    private static int BannerProtectionRangeZ = 50;

    [ModLoader.ModCallback(ModLoader.EModCallbackType.OnAssemblyLoaded, "scarabol.spawnprotect.assemblyload")]
    public static void OnAssemblyLoaded(string path)
    {
      JSONNode jsonConfig;
      if (Pipliz.JSON.JSON.Deserialize(Path.Combine(Path.GetDirectoryName(path), "protection-ranges.json"), out jsonConfig, false)) {
        if (!jsonConfig.TryGetAs<int>("SpawnProtectionRangeX", out SpawnProtectionRangeX)) {
          Pipliz.Log.Write(string.Format("Could not get spawn protection x-range from json config, using default value {0}", SpawnProtectionRangeX));
        }
        if (!jsonConfig.TryGetAs<int>("SpawnProtectionRangeZ", out SpawnProtectionRangeZ)) {
          Pipliz.Log.Write(string.Format("Could not get spawn protection z-range from json config, using default value {0}", SpawnProtectionRangeZ));
        }
        if (!jsonConfig.TryGetAs<int>("BannerProtectionRangeX", out BannerProtectionRangeX)) {
          Pipliz.Log.Write(string.Format("Could not get banner protection x-range from json config, using default value {0}", BannerProtectionRangeX));
        }
        if (!jsonConfig.TryGetAs<int>("BannerProtectionRangeZ", out BannerProtectionRangeZ)) {
          Pipliz.Log.Write(string.Format("Could not get banner protection z-range from json config, using default value {0}", BannerProtectionRangeZ));
        }
      } else {
        Pipliz.Log.Write("Could not find protection-ranges.json file");
      }
      Pipliz.Log.Write(string.Format("Using spawn protection with x-range {0}", SpawnProtectionRangeX));
      Pipliz.Log.Write(string.Format("Using spawn protection with z-range {0}", SpawnProtectionRangeZ));
      Pipliz.Log.Write(string.Format("Using banner protection with x-range {0}", BannerProtectionRangeX));
      Pipliz.Log.Write(string.Format("Using banner protection with z-range {0}", BannerProtectionRangeZ));
    }

    [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterStartup, "scarabol.spawnprotect.registercallbacks")]
    public static void AfterStartup()
    {
      Pipliz.Log.Write("Loaded SpawnProtect Mod 0.9.2 by Scarabol");
    }

    [ModLoader.ModCallback(ModLoader.EModCallbackType.OnTryChangeBlockUser, "scarabol.spawnprotect.trychangeblock")]
    public static bool OnTryChangeBlockUser(ModLoader.OnTryChangeBlockUserData d) {
      Players.Player requestedBy = d.requestedBy;
      Vector3Int position = d.VoxelToChange;
      Vector3Int spawn = TerrainGenerator.GetSpawnLocation();
      if (System.Math.Abs(spawn.x - position.x) <= SpawnProtectionRangeX && System.Math.Abs(spawn.z - position.z) <= SpawnProtectionRangeZ) {
        if (!PermissionsManager.HasPermission(requestedBy, PERMISSION_SPAWN_CHANGE)) {
          Chat.Send(requestedBy, "<color=red>You don't have permission to change the spawn area!</color>");
          // TODO add counter and report to admins
          return false;
        }
      } else {
        int checkRangeX = BannerProtectionRangeX;
        int checkRangeZ = BannerProtectionRangeZ;
        if (d.typeToBuild == ItemTypes.IndexLookup.GetIndex("banner")) {
          checkRangeX *= 2;
          checkRangeZ *= 2;
        }
        foreach (Banner b in BannerTracker.GetBanners()) {
          Vector3Int bannerLocation = b.KeyLocation;
          if (System.Math.Abs(bannerLocation.x - position.x) <= checkRangeX && System.Math.Abs(bannerLocation.z - position.z) <= checkRangeZ) {
            if (b.Owner != requestedBy && !PermissionsManager.HasPermission(requestedBy, PERMISSION_BANNER_PREFIX + b.Owner.ID.steamID)) {
              Chat.Send(requestedBy, "<color=red>You don't have permission to change this area!</color>");
              return false;
            }
            break;
          }
        }
      }
      return true;
    }

    [ModLoader.ModCallback(ModLoader.EModCallbackType.AfterItemTypesServer, "scarabol.spawnprotect.registertypes")]
    public static void AfterItemTypesServer()
    {
      ChatCommands.CommandManager.RegisterCommand(new SpawnProtectChatCommand());
    }
  }

  public class SpawnProtectChatCommand : ChatCommands.IChatCommand
  {
    public bool IsCommand(string chat)
    {
      return chat.StartsWith("/spawnprotect ");
    }

    public bool TryDoCommand(Players.Player causedBy, string chattext)
    {
      var matched = Regex.Match(chattext, @"/spawnprotect (?<access>.+) (?<steamid>.+)");
      if (!matched.Success) {
        Chat.Send(causedBy, "Command didn't match, use /spawnprotect [spawn|nospawn|banner|deny] steamid");
        return true;
      }
      string access = matched.Groups["access"].Value;
      ulong steamid;
      if (!ulong.TryParse(matched.Groups["steamid"].Value, out steamid)) {
        Chat.Send(causedBy, "Failed parsing steamid");
        return true;
      }
      Steamworks.CSteamID csteamid = new Steamworks.CSteamID(steamid);
      if (!csteamid.IsValid()) {
        Chat.Send(causedBy, "steamid is not valid");
        return true;
      }
      NetworkID networkId = new NetworkID(csteamid);
      Players.Player targetPlayer;
      if (!Players.TryGetPlayer(networkId, out targetPlayer)) {
        Chat.Send(causedBy, "Player not found or offline");
        return true;
      }
      if (access.Equals("spawn")) {
        if (!PermissionsManager.CheckAndWarnPermission(causedBy, SpawnProtectModEntries.PERMISSION_SUPER)) {
          return true;
        }
        PermissionsManager.AddPermissionToUser(causedBy, targetPlayer, SpawnProtectModEntries.PERMISSION_SPAWN_CHANGE);
      } else if (access.Equals("nospawn")) {
        if (PermissionsManager.HasPermission(causedBy, SpawnProtectModEntries.PERMISSION_SUPER)) {
          PermissionsManager.RemovePermissionOfUser(causedBy, targetPlayer, SpawnProtectModEntries.PERMISSION_SPAWN_CHANGE);
        }
      } else if (access.Equals("banner")) {
        PermissionsManager.AddPermissionToUser(causedBy, targetPlayer, SpawnProtectModEntries.PERMISSION_BANNER_PREFIX + causedBy.ID.steamID);
      } else if (access.Equals("deny")) {
        PermissionsManager.RemovePermissionOfUser(causedBy, targetPlayer, SpawnProtectModEntries.PERMISSION_BANNER_PREFIX + causedBy.ID.steamID);
      } else {
        Chat.Send(causedBy, "Unknown access level, use /spawnprotect [spawn|nospawn|banner|deny] steamid");
      }
      return true;
    }
  }
}
