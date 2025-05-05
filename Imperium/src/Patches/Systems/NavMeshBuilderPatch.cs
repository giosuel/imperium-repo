using HarmonyLib;
using UnityEngine.AI;

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(NavMeshBuilder))]
public class NavMeshBuilderPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("UpdateNavMeshData")]
    private static void UpdateNavMeshDataPatch()
    {
        Imperium.Visualization.NavMeshVisualizer.Refresh();
    }

    [HarmonyPostfix]
    [HarmonyPatch("UpdateNavMeshDataAsync")]
    private static void UpdateNavMeshDataAsyncPatch()
    {
        Imperium.Visualization.NavMeshVisualizer.Refresh();
    }
}