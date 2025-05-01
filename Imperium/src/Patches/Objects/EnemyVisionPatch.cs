using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using Imperium.Util;
using MonoMod.Utils;
using Photon.Pun;
using RepoSteamNetworking.API;

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyVision))]
internal static class EnemyVisionPatch
{
    private static List<PlayerAvatar> playerListBackup;

    [HarmonyPostfix]
    [HarmonyPatch("Vision")]
    private static IEnumerator VisionPatch(IEnumerator result, EnemyVision __instance)
    {
        while (result.MoveNext())
        {
            // Restore original player list after vision update
            if (PhotonNetwork.IsMasterClient && playerListBackup != null)
            {
                GameDirector.instance.PlayerList = playerListBackup;
            }

            yield return result.Current;

            // Replace the player list for the next iteration to exclude invisible players
            if (PhotonNetwork.IsMasterClient)
            {
                playerListBackup = GameDirector.instance.PlayerList;
                GameDirector.instance.PlayerList = GameDirector.instance.PlayerList
                    .Where(player => !Imperium.PlayerManager.InvisiblePlayers.Value.Contains(player.GetSteamId()))
                    .ToList();
            }

            Imperium.Visualization.EnemyGizmos.VisionUpdate(__instance);
        }
    }
}