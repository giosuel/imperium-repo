#region

using System;
using System.Collections.Generic;
using System.Linq;
using AsmResolver.PE.Imports;
using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.ArenaControl;
using Imperium.Interface.ImperiumUI.Windows.ControlCenter;
using Imperium.Interface.ImperiumUI.Windows.EventLog;
using Imperium.Interface.ImperiumUI.Windows.LevelGeneration;
using Imperium.Interface.ImperiumUI.Windows.ObjectExplorer;
using Imperium.Interface.ImperiumUI.Windows.Portal;
using Imperium.Interface.ImperiumUI.Windows.Preferences;
using Imperium.Interface.ImperiumUI.Windows.Rendering;
using Imperium.Interface.ImperiumUI.Windows.Teleport;
using Imperium.Interface.ImperiumUI.Windows.Upgrades;
using Imperium.Interface.ImperiumUI.Windows.Visualization;
using Imperium.Types;
using Imperium.Util;
using Librarium.Binding;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using Vector2 = System.Numerics.Vector2;

#endregion

namespace Imperium.Interface.ImperiumUI;

public class ImperiumUI : BaseUI
{
    private readonly Dictionary<Type, WindowDefinition> windowControllers = [];
    private readonly Dictionary<Type, ImpBinding<bool>> dockButtonBindings = [];
    private readonly Dictionary<Type, IBinding<bool>[]> canOpenBindingMap = [];

    private readonly ImpStack<WindowDefinition> controllerStack = [];

    private RectTransform dockRect;

    // ReSharper disable Unity.PerformanceAnalysis
    protected override void InitUI()
    {
        dockRect = container.Find("Dock").GetComponent<RectTransform>();

        RegisterImperiumWindow<ControlCenterWindow>(
            ImpAssets.ControlCenterWindowObject,
            "Left/ControlCenter",
            "Imperium Control Center"
        );
        RegisterImperiumWindow<ObjectExplorerWindow>(
            ImpAssets.ObjectExplorerWindowObject,
            "Left/ObjectExplorer",
            "Object Explorer",
            canOpenBindings: Imperium.IsGameLevel
        );

        RegisterImperiumWindow<VisualizationWindow>(
            ImpAssets.VisualizationWindowObject,
            "Center/Visualization",
            "Visualization",
            canOpenBindings: Imperium.IsGameLevel
        );
        RegisterImperiumWindow<TeleportWindow>(
            ImpAssets.TeleportationWindowObject,
            "Center/Teleportation",
            "Teleportation",
            keybind: Imperium.InputBindings.InterfaceMap.TeleportWindow,
            canOpenBindings: Imperium.IsGameLevel
        );
        RegisterImperiumWindow<LevelGeneration>(
            ImpAssets.LevelGenerationWindowObject,
            "Center/LevelGeneration",
            "Level Generation"
        );
        RegisterImperiumWindow<UpgradesWindow>(
            ImpAssets.UpgradesWindowObject,
            "Center/Upgrades",
            "Upgrades",
            canOpenBindings: Imperium.IsGameLevel
        );
        RegisterImperiumWindow<ArenaControlWindow>(
            ImpAssets.ArenaControlWindowObject,
            "Center/ArenaControl",
            "Arena Control",
            canOpenBindings: Imperium.IsGameLevel
        );

        RegisterImperiumWindow<RenderingWindow>(
            ImpAssets.RenderingWindowObject,
            "Right/Rendering",
            "Render Settings"
        );
        // RegisterImperiumWindow<SaveEditorWindow>(
        //     ImpAssets.SaveEditorWindowObject,
        //     "Right/SaveEditor",
        //     "Save File Editor"
        // );
        RegisterImperiumWindow<EventLogWindow>(
            ImpAssets.EventLogWindowObject,
            "Right/EventLog",
            "Event Log"
        );
        // RegisterImperiumWindow<InfoWindow>(
        //     ImpAssets.InfoWindowObject,
        //     "Right/Info",
        //     "Level Information",
        //     canOpenBindings: Imperium.IsArenaLoaded
        // );
        RegisterImperiumWindow<PortalWindow>(
            ImpAssets.PortalWindowObject,
            "Right/Portal",
            "Portals"
        );
        RegisterImperiumWindow<PreferencesWindow>(
            ImpAssets.PreferencesWindowObject,
            "Right/Preferences",
            "Imperium Preferences"
        );

        LoadLayout();
    }

    protected override void OnThemePrimaryUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container,
            new StyleOverride("Dock/Left", Variant.BACKGROUND),
            new StyleOverride("Dock/Center", Variant.BACKGROUND),
            new StyleOverride("Dock/Right", Variant.BACKGROUND),
            new StyleOverride("Dock/Left/Border", Variant.DARKER),
            new StyleOverride("Dock/Center/Border", Variant.DARKER),
            new StyleOverride("Dock/Right/Border", Variant.DARKER)
        );
    }

    internal T Get<T>() where T : ImperiumWindow
    {
        return (T)windowControllers.FirstOrDefault(controller => controller.Value.Controller is T).Value.Controller;
    }

    private void RegisterImperiumWindow<T>(
        GameObject obj,
        string dockButtonPath,
        string windowName,
        string windowDescription = null,
        InputAction keybind = null,
        params IBinding<bool>[] canOpenBindings
    ) where T : ImperiumWindow
    {
        if (windowControllers.ContainsKey(typeof(T))) return;

        var floatingWindow = Instantiate(obj.transform.Find("Window").gameObject, container).AddComponent<T>();

        var windowDefinition = new WindowDefinition
        {
            Controller = floatingWindow,
            IsOpen = false,
            WindowType = typeof(T)
        };
        windowControllers[typeof(T)] = windowDefinition;
        canOpenBindingMap[typeof(T)] = canOpenBindings;

        floatingWindow.InitWindow(theme, windowDefinition, tooltip, this);
        floatingWindow.onClose += OnCloseWindow<T>;
        floatingWindow.onOpen += OnOpenWindow<T>;
        floatingWindow.onFocus += OnFocusWindow<T>;

        var buttonObj = container.Find("Dock").Find(dockButtonPath);

        /*
         * The border around the dock button indication whether a window is currently open or not
         *
         * Since all can-open-bindings have to be set to true in order for the window to be open, we disable the border
         * if any of the bindings is set to false.
         */
        var buttonImage = buttonObj.GetComponent<Image>();
        buttonImage.enabled = windowDefinition.IsOpen && canOpenBindings.All(binding => binding.Value);

        ImpButton.Bind(
            "",
            buttonObj,
            () =>
            {
                if (windowDefinition.IsOpen)
                {
                    windowDefinition.Controller.Close();
                }
                else
                {
                    windowDefinition.Controller.Open();
                }

                buttonImage.enabled = windowDefinition.IsOpen && canOpenBindings.All(binding => binding.Value);
            },
            isIconButton: true,
            playClickSound: false,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Tooltip = tooltip,
                Title = windowName,
                Description = windowDescription,
                HasAccess = true
            },
            interactableBindings: canOpenBindings
        );
        var buttonBinding = new ImpBinding<bool>(false);
        dockButtonBindings[typeof(T)] = buttonBinding;

        buttonBinding.OnUpdate += value => buttonImage.enabled = value && canOpenBindings.All(binding => binding.Value);

        foreach (var canOpenBinding in canOpenBindings)
        {
            canOpenBinding.OnTrigger += () =>
            {
                buttonImage.enabled = windowDefinition.IsOpen && canOpenBindings.All(binding => binding.Value);
            };
        }

        if (keybind != null)
        {
            floatingWindow.openKeybind = keybind;
            floatingWindow.openKeybind.performed += windowDefinition.Controller.OnKeybindOpen;
        }
    }

    protected override void OnClose() => SaveLayout();

    protected override void OnOpen(bool wasOpen)
    {
        if (dockRect && !wasOpen)
        {
            StartCoroutine(ImpUtils.Interface.SlideInAnimation(dockRect, UnityEngine.Vector2.down));
        }

        foreach (var windowDefinition in controllerStack.Where(controller => controller.IsOpen))
        {
            var canOpen = canOpenBindingMap.TryGetValue(windowDefinition.WindowType, out var bindings) &&
                          bindings.All(binding => binding.Value);
            if (canOpen)
            {
                windowDefinition.Controller.InvokeOnOpen();
            }
            else
            {
                windowDefinition.Controller.Close();

                /*
                 * Since the window is only closed temporarily, we don't want to actually set it to closed.
                 */
                windowDefinition.IsOpen = true;
            }
        }
    }

    private void OnFocusWindow<T>() => controllerStack.MoveToBackOrAdd(windowControllers[typeof(T)]);
    private void OnOpenWindow<T>() => dockButtonBindings[typeof(T)].Set(true);
    private void OnCloseWindow<T>() => dockButtonBindings[typeof(T)].Set(false);

    private void SaveLayout()
    {
        Imperium.Settings.Preferences.ImperiumWindowLayout.Set(JsonConvert.SerializeObject(controllerStack));
    }

    private void LoadLayout()
    {
        var layoutConfigString = Imperium.Settings.Preferences.ImperiumWindowLayout.Value;
        if (string.IsNullOrEmpty(layoutConfigString)) return;

        if (!ImpUtils.DeserializeJsonSafe<List<WindowDefinition>>(layoutConfigString, out var configList))
        {
            Imperium.IO.LogError("[UI] Failed to load ImperiumUI layout config. Invalid JSON detected.");
            return;
        }

        controllerStack.Clear();
        var controllers = new HashSet<Type>();

        foreach (var windowDefinition in configList)
        {
            if (!windowControllers.TryGetValue(windowDefinition.WindowType, out var existingDefinition)) continue;
            if (!controllers.Add(existingDefinition.WindowType)) continue;

            // Propagate data from config to existing definition and add it to the stack
            existingDefinition.IsOpen = windowDefinition.IsOpen;
            existingDefinition.Position = windowDefinition.Position;
            existingDefinition.ScaleFactor = windowDefinition.ScaleFactor;
            controllerStack.Add(existingDefinition);

            // Update the dock button binding
            dockButtonBindings[existingDefinition.WindowType].Set(windowDefinition.IsOpen);

            Imperium.IO.LogInfo($"window: {windowDefinition.WindowType.Name} is open: {windowDefinition.IsOpen}");

            // Inform the window of the new state
            existingDefinition.Controller.PlaceWindow(
                windowDefinition.Position,
                windowDefinition.ScaleFactor,
                windowDefinition.IsOpen
            );
        }
    }
}

public record WindowDefinition
{
    internal ImperiumWindow Controller { get; init; }
    public Type WindowType { get; init; }
    public Vector2 Position { get; set; }
    public float ScaleFactor { get; set; } = 1;
    public bool IsOpen { get; set; }
}