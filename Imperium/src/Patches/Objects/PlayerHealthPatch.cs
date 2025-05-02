#region

using HarmonyLib;

#endregion

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(PlayerHealth))]
internal static class PlayerHealthPatch
{
}