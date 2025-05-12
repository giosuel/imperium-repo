#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(MenuCursor))]
internal static class MenuCursorPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Update")]
    private static void UpdatePatch(MenuCursor __instance)
    {
        if (Imperium.Interface.IsOpen()) __instance.mesh.SetActive(false);
    }
}