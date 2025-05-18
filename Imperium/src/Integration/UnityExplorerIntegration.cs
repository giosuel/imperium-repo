#region

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using BepInEx.Bootstrap;
using HarmonyLib;
using Imperium.Util;
using UnityExplorer;
using UniverseLib.UI;

#endregion

namespace Imperium.Integration;

public static class UnityExplorerIntegration
{
    internal static bool IsOpen;

    private static bool IsEnabled => Chainloader.PluginInfos.ContainsKey("com.sinai.unityexplorer");
    private static readonly UnityExplorerBlocker blocker = new();

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void CloseUI()
    {
        if (!IsEnabled) return;

        CloseUIInternal();
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void PatchFunctions(Harmony harmony)
    {
        if (!IsEnabled) return;

        PatchFunctionsInternal(harmony);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    internal static void SetUIActivePatch(string id, bool active)
    {
        if (!IsEnabled) return;

        OnOpenUIInternal(id, active);
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void CloseUIInternal()
    {
        // If this is called before the UE window is registered it will throw an error
        try
        {
            UniversalUI.SetUIActive(ExplorerCore.GUID, false);
        }
        catch (ArgumentException)
        {
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void PatchFunctionsInternal(Harmony harmony)
    {
        harmony.Patch(
            typeof(UniversalUI).GetMethod("SetUIActive"),
            prefix: new HarmonyMethod(
                typeof(UnityExplorerIntegration)
                    .GetMethod(nameof(SetUIActivePatch), bindingAttr: BindingFlags.NonPublic | BindingFlags.Static)
            )
        );
    }

    [MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
    private static void OnOpenUIInternal(string id, bool active)
    {
        if (ExplorerCore.GUID == id)
        {
            if (Imperium.Settings.Preferences.UnityExplorerMouseFix.Value)
            {
                ImpUtils.Interface.ToggleCursorState(active);
            }

            if (active)
            {
                Imperium.Interface.Close(false);
                Imperium.InputBlocker.Block(blocker);

                Imperium.InputBindings.BaseMap.Freecam.Disable();
                Imperium.InputBindings.BaseMap.Minimap.Disable();
                Imperium.InputBindings.BaseMap.Minicam.Disable();
                Imperium.InputBindings.BaseMap.Teleport.Disable();
                Imperium.InputBindings.BaseMap.TapeMeasure.Disable();
            }
            else
            {
                Imperium.InputBlocker.Unblock(blocker);

                Imperium.InputBindings.BaseMap.Freecam.Enable();
                Imperium.InputBindings.BaseMap.Minimap.Enable();
                Imperium.InputBindings.BaseMap.Minicam.Enable();
                Imperium.InputBindings.BaseMap.Teleport.Enable();
                Imperium.InputBindings.BaseMap.TapeMeasure.Enable();
            }

            IsOpen = active;
        }
    }
}

internal class UnityExplorerBlocker;