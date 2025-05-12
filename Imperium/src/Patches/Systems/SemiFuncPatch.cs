#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Systems;

[HarmonyPatch(typeof(SemiFunc))]
internal static class SemiFuncPatch
{
    // [HarmonyPostfix]
    // [HarmonyPatch("InputMovementY")]
    // private static void InputMovementYPatch(ref float __result)
    // {
    //     if (Imperium.Freecam.IsFreecamEnabled.Value || Imperium.Interface.IsOpen()) __result = 0;
    // }
    //
    // [HarmonyPostfix]
    // [HarmonyPatch("InputMovementX")]
    // private static void InputMovementXPatch(ref float __result)
    // {
    //     if (Imperium.Freecam.IsFreecamEnabled.Value || Imperium.Interface.IsOpen()) __result = 0;
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch("InputMouseX")]
    // private static void InputMouseXPatch(ref float __result)
    // {
    //     if (Imperium.Freecam.IsFreecamEnabled.Value || Imperium.Interface.IsOpen()) __result = 0;
    // }
    //
    // [HarmonyPostfix]
    // [HarmonyPatch("InputMouseY")]
    // private static void InputMouseYPatch(ref float __result)
    // {
    //     if (Imperium.Freecam.IsFreecamEnabled.Value || Imperium.Interface.IsOpen()) __result = 0;
    // }

    // [HarmonyPostfix]
    // [HarmonyPatch("InputScrollY")]
    // private static void InputScrollYPatch(ref float __result)
    // {
    //     if (Imperium.Freecam.IsFreecamEnabled.Value || Imperium.Interface.IsOpen()) __result = 0;
    // }
}