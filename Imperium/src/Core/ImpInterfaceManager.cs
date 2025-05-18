#region

using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using HarmonyLib;
using Imperium.Integration;
using Imperium.Interface;
using Imperium.Types;
using Imperium.Util;
using Librarium.Binding;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core;

internal class ImpInterfaceManager : MonoBehaviour
{
    private ImpTooltip tooltip;
    private readonly Dictionary<Type, BaseUI> interfaceControllers = [];

    internal readonly ImpBinding<BaseUI> OpenInterface = new();
    internal ImpBinding<ImpTheme> Theme { get; private set; }

    // We implement the exiting from UIs with bepinex controls as we have to differentiate
    // between the Escape and Tab character due to some UIs overriding the tab button callback.
    // Unfortunately the native OpenMenu action can be either of the keys.
    private KeyboardShortcut escShortcut = new(KeyCode.Escape);

    internal static ImpInterfaceManager Create(ImpConfig<string> themeConfig, Transform parent)
    {
        var interfaceManager = new GameObject("Imp_Interface").AddComponent<ImpInterfaceManager>();
        interfaceManager.transform.SetParent(parent);
        interfaceManager.Theme = ImpThemeManager.BindTheme(themeConfig);

        // Instantiate Imperium Tooltip
        interfaceManager.tooltip = Instantiate(
            ImpAssets.ImperiumTooltipObject, interfaceManager.transform
        ).AddComponent<ImpTooltip>();
        interfaceManager.tooltip.Init(interfaceManager.Theme, interfaceManager.tooltip);
        interfaceManager.tooltip.gameObject.SetActive(false);

        Imperium.IsLevelLoaded.OnTrigger += interfaceManager.InvokeOnOpen;

        return interfaceManager;
    }

    private void Update()
    {
        if (!OpenInterface.Value) return;

        if (escShortcut.IsDown())
        {
            UnityExplorerIntegration.CloseUI();
            Close();
        }
    }

    internal void RegisterInterface<T>(
        GameObject obj,
        InputAction keybind = null,
        params IBinding<bool>[] canOpenBindings
    ) where T : BaseUI
    {
        if (interfaceControllers.ContainsKey(typeof(T))) return;

        var interfaceObj = Instantiate(obj, transform).AddComponent<T>();
        interfaceObj.InitUI(Theme, tooltip, canOpenBindings);

        interfaceObj.interfaceManager = this;
        interfaceControllers[typeof(T)] = interfaceObj;

        if (keybind != null)
        {
            keybind.performed += Toggle<T>;
            keybinds.Add((keybind, Toggle<T>));
        }
    }

    private readonly List<(InputAction, Action<InputAction.CallbackContext>)> keybinds = [];

    private void OnDestroy()
    {
        foreach (var (action, function) in keybinds) action.performed -= function;
    }

    public void RefreshTheme() => Theme.Refresh();

    public void Unregister<T>()
    {
        Destroy(interfaceControllers[typeof(T)]);
        interfaceControllers.Remove(typeof(T));
    }

    public T Get<T>() where T : BaseUI
    {
        return (T)interfaceControllers.FirstOrDefault(controller => controller.Value is T).Value;
    }

    public bool IsOpen<T>() where T : BaseUI
    {
        var controller = (T)interfaceControllers.FirstOrDefault(controller => controller.Value is T).Value;
        return controller && controller.IsOpen;
    }

    public void Open<T>(bool toggleCursorState = true, bool closeOthers = true)
    {
        Open(typeof(T), toggleCursorState, closeOthers);
    }

    public void Close() => Close(true);

    public void Destroy()
    {
        Close();
        foreach (var controller in interfaceControllers.Values) Destroy(controller);
        Destroy(gameObject);
    }

    public void ResetUI()
    {
        Close();
        Imperium.Settings.Preferences.ImperiumWindowLayout.Reset();
        Imperium.Reload();
    }

    public bool IsOpen() => OpenInterface.Value;

    public void Close(bool toggleCursorState)
    {
        if (!OpenInterface.Value) return;

        OpenInterface.Value.OnUIClose();
        OpenInterface.Set(null);

        if (toggleCursorState)
        {
            tooltip.Deactivate();
            ImpUtils.Interface.ToggleCursorState(false);
        }

        Imperium.InputBlocker.Unblock(this);
    }

    public void Toggle<T>(InputAction.CallbackContext _) => Toggle<T>();

    public void Toggle<T>(bool toggleCursorState = true, bool closeOthers = true)
    {
        if (!interfaceControllers.TryGetValue(typeof(T), out var controller)) return;

        if (controller.IsOpen)
        {
            Close(toggleCursorState);
        }
        else if (controller.CanOpenBindings.All(binding => binding.Value))
        {
            Open<T>(toggleCursorState, closeOthers);
        }
    }

    private void InvokeOnOpen()
    {
        if (OpenInterface.Value) OpenInterface.Value.InvokeOnOpen(true);
    }

    public void Open(Type type, bool toggleCursorState = true, bool closeOthers = true)
    {
        if (!interfaceControllers.TryGetValue(type, out var controller))
        {
            Imperium.IO.LogError($"[Interface] Failed to open interface {type}");
            return;
        }

        if (controller.IsOpen || !controller.CanOpen()) return;

        controller.OnUIOpen();

        OpenInterface.Set(controller);

        // Close Unity Explorer menus
        UnityExplorerIntegration.CloseUI();

        // Close the menu only if current level is not a menu level
        if (Imperium.IsGameLevel.Value) MenuManager.instance.PageCloseAll();

        if (closeOthers)
        {
            tooltip.Deactivate();

            interfaceControllers.Values
                .Where(interfaceController => interfaceController != controller && interfaceController)
                .Do(interfaceController => interfaceController.OnUIClose());
        }

        if (toggleCursorState) ImpUtils.Interface.ToggleCursorState(true);
        Imperium.InputBlocker.Block(this);
    }

    private void OnDisable() => Imperium.InputBlocker.Unblock(this);
}