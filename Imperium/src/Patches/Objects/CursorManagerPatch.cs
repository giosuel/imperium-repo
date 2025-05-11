#region

using HarmonyLib;
using UnityEngine;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(CursorManager))]
internal static class CursorManagerPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("Unlock")]
    private static void UnlockPatch(CursorManager __instance, float _time)
    {
        if (Imperium.Interface.IsOpen()) Cursor.visible = true;
    }
}