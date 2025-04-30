using System.Linq;
using HarmonyLib;
using Photon.Pun;
using UnityEngine;

namespace Imperium.Patches.Objects;

[HarmonyPatch(typeof(DefaultPool))]
internal static class DefaultPoolPatch
{
    /// <summary>
    /// To make sure that we can load any module from any level we need to trick the game a bit.
    ///
    /// The game spawns modules with the resources path, which is level specific.
    /// By checking if the resources path would be invalid and preemptively injecting the module object to the spawn pool's
    /// cache we can bypass the internal resources check.
    /// </summary>
    [HarmonyPrefix]
    [HarmonyPatch("Instantiate")]
    private static void InstantiatePatch(DefaultPool __instance, string prefabId)
    {
        if (Resources.Load<GameObject>(prefabId) == null)
        {
            var moduleName = prefabId.Split('/').Last();
            foreach (var module in Imperium.ObjectManager.LoadedModules.Value)
            {
                if (module.name == moduleName)
                {
                    __instance.ResourceCache.TryAdd(prefabId, module.gameObject);
                    break;
                }
            }
        }
    }
}