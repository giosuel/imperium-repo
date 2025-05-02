#region

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RepoSteamNetworking.API;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(EnemyVision))]
internal static class EnemyVisionPatch
{
    [HarmonyTranspiler]
    [HarmonyPatch("Vision", MethodType.Enumerator)]
    private static IEnumerable<CodeInstruction> VisionTranspiler(IEnumerable<CodeInstruction> instructions)
    {
        return new CodeMatcher(instructions)
            .MatchForward(
                true,
                new CodeMatch(
                    OpCodes.Ldfld, AccessTools.Field(typeof(GameDirector), nameof(GameDirector.PlayerList))
                )
            )
            .Repeat(match => match
                .Advance(1)
                .Insert(Transpilers.EmitDelegate(GetPlayerList))
            )
            .Instructions();
    }

    [HarmonyPostfix]
    [HarmonyPatch("Vision")]
    private static IEnumerator Vision(IEnumerator result, EnemyVision __instance)
    {
        while (result.MoveNext())
        {
            yield return result.Current;

            Imperium.Visualization.EnemyGizmos.VisionUpdate(__instance);
        }
    }

    private static List<PlayerAvatar> GetPlayerList(List<PlayerAvatar> players)
    {
        return players
            .Where(player => !Imperium.PlayerManager.InvisiblePlayers.Value.Contains(player.GetSteamId()))
            .ToList();
    }
}