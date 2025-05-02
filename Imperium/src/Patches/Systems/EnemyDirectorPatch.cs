#region

using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using RepoSteamNetworking.API;
using UnityEngine;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(EnemyDirector))]
internal static class EnemyDirectorPatch
{
    [HarmonyPrefix]
    [HarmonyPatch("SetInvestigate")]
    private static bool SetInvestigatePrefixPatch(EnemyDirector __instance, Vector3 position, float radius,
        out bool __state)
    {
        if (!SemiFunc.IsMasterClientOrSingleplayer())
        {
            __state = true;
            return true;
        }

        var methodInfo = new StackTrace().GetFrame(2).GetMethod();

        if (methodInfo.ReflectedType == typeof(PlayerAvatar) ||
            methodInfo.ReflectedType == typeof(PlayerVoiceChat))
        {
            var originPlayer = GameDirector.instance.PlayerList.FirstOrDefault(player =>
                position == player.transform.position + Vector3.up * 0.2f ||
                position == player.PlayerVisionTarget.VisionTransform.transform.position
            );

            if (originPlayer && Imperium.PlayerManager.MutedPlayers.Value.Contains(originPlayer.GetSteamId()))
            {
                __state = false;
                return false;
            }
        }

        __state = true;
        return true;
    }

    [HarmonyPostfix]
    [HarmonyPatch("SetInvestigate")]
    private static void SetInvestigatePostfixPatch(EnemyDirector __instance, Vector3 position, float radius, bool __state)
    {
        Imperium.Visualization.NoiseIndicators.AddNoise(position, radius, !__state);
    }
}