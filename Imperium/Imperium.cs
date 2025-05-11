#region

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Imperium.Core;
using Imperium.Core.EventLogging;
using Imperium.Core.Input;
using Imperium.Core.Lifecycle;
using Imperium.Core.Portal;
using Imperium.Core.Scripts;
using Imperium.Integration;
using Imperium.Interface.ImperiumUI;
using Imperium.Interface.MapUI;
using Imperium.Interface.SpawningUI;
using Imperium.Networking;
using Imperium.Patches;
using Imperium.Util;
using Librarium.Binding;
using UnityEngine;
using ImpSettings = Imperium.Core.ImpSettings;

#endregion

namespace Imperium;

[BepInDependency("com.rune580.reposteamnetworking")]
[BepInDependency("REPOLib")]
[BepInDependency("giosuel.Librarium")]
[BepInDependency("com.sinai.universelib", BepInDependency.DependencyFlags.SoftDependency)]
[BepInDependency("com.sinai.unityexplorer", BepInDependency.DependencyFlags.SoftDependency)]
[BepInPlugin(LCMPluginInfo.PLUGIN_GUID, LCMPluginInfo.PLUGIN_NAME, LCMPluginInfo.PLUGIN_VERSION)]
public class Imperium : BaseUnityPlugin
{
    private static Harmony Harmony;
    private static ManualLogSource Log;
    private static ConfigFile configFile;

    /*
     * Relays to vanilla singletons. This makes tracking Imperium singleton access easier.
     */
    internal static MenuManager MenuManager => MenuManager.instance;
    internal static PlayerAvatar Player => PlayerAvatar.instance;
    internal static GameDirector GameDirector => GameDirector.instance;

    /*
     * Preload systems. Instantiated when Imperium is initialized (Stage 1).
     */
    internal static ImpSettings Settings { get; private set; }
    internal static ImpOutput IO { get; private set; }
    internal static ImpNetworking Networking { get; set; }
    internal static StartupManager StartupManager { get; private set; }
    internal static PortalManager PortalManager { get; private set; }

    /*
     * Lifecycle systems. Instantiated when Imperium is launched (Stage 2).
     */
    internal static Core.Lifecycle.GameManager GameManager { get; private set; }
    internal static ObjectManager ObjectManager { get; private set; }
    internal static PlayerManager PlayerManager { get; private set; }
    internal static ArenaManager ArenaManager { get; private set; }
    internal static Visualization Visualization { get; private set; }
    internal static ImpEventLog EventLog { get; private set; }
    internal static InputBlocker InputBlocker { get; private set; }

    /*
     * Imperium game objects and world-space managers. Instantiated when Imperium is launched (Stage 2).
     */
    internal static ImpMap Map { get; private set; }
    internal static ImpFreecam Freecam { get; private set; }
    internal static ImpNightVision NightVision { get; private set; }
    internal static TapeMeasure TapeMeasure { get; private set; }
    internal static ImpInputBindings InputBindings { get; private set; }
    internal static PositionIndicator PositionIndicator { get; private set; }
    internal static ImpInterfaceManager Interface { get; private set; }
    internal static WaypointManager WaypointManager { get; private set; }

    /// <summary>
    ///     Set to true when Imperium is initialized (Stage 1).
    /// </summary>
    internal static bool IsImperiumInitialized { get; private set; }

    /// <summary>
    ///     Set to true when Imperium is launched (Stage 2) and ready to serve API calls.
    /// </summary>
    internal static bool IsImperiumLaunched { get; private set; }

    /// <summary>
    ///     Set to true when Imperium is launched and imperium access is currently granted.
    /// </summary>
    internal static ImpBinaryBinding IsImperiumEnabled { get; private set; }

    /// <summary>
    ///     Binding that indicates whether the game has finished loading the current level.
    ///
    ///     Triggers after every call to <see cref="LoadingUI.StartLoading"/> and <see cref="LoadingUI.StopLoading"/>.
    /// </summary>
    internal static ImpBinaryBinding IsLevelLoaded { get; private set; }

    /// <summary>
    /// Binding that indicates whether the currently loaded level is a game level and not the main menu or lobby.
    ///
    /// Is updated every time <see cref="IsLevelLoaded"/> is udpated.
    /// </summary>
    internal static IBinding<bool> IsGameLevel { get; private set; }

    internal static GameObject GameObject { get; private set; }

    internal static ImpBinding<Camera> ActiveCamera;

    /// <summary>
    ///     Imperium initialization (Stage 1)
    ///     This happens as soon as BepInEx loads the Imperium plugin.
    /// </summary>
    private void Awake()
    {
        configFile = Config;
        Log = Logger;

        IsLevelLoaded = new ImpBinaryBinding(false);
        IsGameLevel = new ImpExternalBinding<bool, bool>(Core.Lifecycle.GameManager.IsGameLevel, IsLevelLoaded);

        IsLevelLoaded.OnUpdate += value => IO.LogInfo($"Is level loaded : {value}");
        IsGameLevel.OnUpdate += value => IO.LogInfo($"Is game level : {value}");

        IsImperiumEnabled = new ImpBinaryBinding(false);

        Settings = new ImpSettings(Config);
        IsLevelLoaded.OnUpdate += OnLevelLoaded;

        IO = new ImpOutput(Log);
        StartupManager = new StartupManager();
        Networking = new ImpNetworking();
        PortalManager = new PortalManager();
        PortalManager.RegisterTestPortal();

        if (!ImpAssets.Load()) return;

        Harmony = new Harmony(LCMPluginInfo.PLUGIN_VERSION);
        PreLaunchPatches();

        IsImperiumInitialized = true;
        IO.LogInfo("[INIT] Imperium has been successfully initialized \\o/");
    }

    /// <summary>
    ///     Imperium launch (Stage 2)
    ///     This is executed after Imperium access has been granted by the host.
    /// </summary>
    internal static void Launch()
    {
        if (!IsImperiumInitialized || IsImperiumLaunched) return;

        GameObject = new GameObject("Imperium");
        DontDestroyOnLoad(GameObject);

        InputBlocker = new InputBlocker();
        InputBindings = new ImpInputBindings();

        // Re-instantiate settings to get rid of existing bindings
        // IsArenaLoaded = new ImpBinaryBinding(false);
        // Settings = new ImpSettings(configFile);

        // Register camera update when scene is loaded
        ActiveCamera = new ImpExternalBinding<Camera, bool>(() => PlayerAvatar.instance.localCamera, IsLevelLoaded);

        IO.BindNotificationSettings(Settings);
        Networking.BindAllowClients(Settings.Preferences.AllowClients);

        Interface = ImpInterfaceManager.Create(Settings.Preferences.Theme, GameObject.transform);
        EventLog = new ImpEventLog();

        GameManager = ImpLifecycleObject.Create<Core.Lifecycle.GameManager>(
            GameObject.transform, IsLevelLoaded, ImpNetworking.ConnectedPlayers
        );
        ArenaManager = ImpLifecycleObject.Create<ArenaManager>(
            GameObject.transform, IsLevelLoaded, ImpNetworking.ConnectedPlayers
        );
        ObjectManager = ImpLifecycleObject.Create<ObjectManager>(
            GameObject.transform, IsLevelLoaded, ImpNetworking.ConnectedPlayers
        );
        PlayerManager = ImpLifecycleObject.Create<PlayerManager>(
            GameObject.transform, IsLevelLoaded, ImpNetworking.ConnectedPlayers
        );
        WaypointManager = ImpLifecycleObject.Create<WaypointManager>(
            GameObject.transform, IsLevelLoaded, ImpNetworking.ConnectedPlayers
        );

        Visualization = new Visualization(GameObject.transform, ObjectManager, configFile);

        Map = ImpScript.Create<ImpMap>(GameObject.transform);
        Freecam = ImpScript.Create<ImpFreecam>(GameObject.transform);
        NightVision = ImpScript.Create<ImpNightVision>(GameObject.transform);
        TapeMeasure = ImpScript.Create<TapeMeasure>(
            GameObject.transform, ImpAssets.TapeIndicatorObject
        );
        PositionIndicator = ImpScript.Create<PositionIndicator>(
            GameObject.transform, ImpAssets.PositionIndicatorObject
        );

        // Patch the rest of the functionality at the end to make sure all the dependencies of the static patch
        // functions are loaded
        Harmony.PatchAll();
        UnityExplorerIntegration.PatchFunctions(Harmony);

        IsImperiumLaunched = true;

        // Enable Imperium frontend if Imperium is enabled in the config
        if (Settings.Preferences.EnableImperium.Value)
        {
            EnableImperium();

            // Send scene update to ensure consistency in the UIs
            IsLevelLoaded.SetFalse();
        }
        else
        {
            DisableImperium();
        }
    }


    internal static void DisableImperium()
    {
        if (!IsImperiumLaunched || !IsImperiumEnabled.Value) return;

        IsImperiumEnabled.SetFalse();

        Interface.Close();
        PlayerManager.IsFlying.SetFalse();
        Freecam.IsFreecamEnabled.SetFalse();
        PositionIndicator.Deactivate();
        TapeMeasure.Deactivate();

        InputBindings.BaseMap.Disable();
        InputBindings.StaticMap.Disable();
        InputBindings.FreecamMap.Disable();
        InputBindings.InterfaceMap.Disable();
    }

    internal static void EnableImperium()
    {
        if (!IsImperiumLaunched || IsImperiumEnabled.Value) return;
        IsImperiumEnabled.SetTrue();

        Settings.LoadAll();

        RegisterInterfaces();

        InputBindings.BaseMap.Enable();
        InputBindings.StaticMap.Enable();
        InputBindings.FreecamMap.Enable();
        InputBindings.InterfaceMap.Enable();
    }

    internal static void Unload()
    {
        if (!IsImperiumLaunched) return;

        Harmony.UnpatchSelf();
        Networking.Unload();

        DisableImperium();
        PreLaunchPatches();

        Destroy(GameObject);

        IsImperiumLaunched = false;
    }

    internal static void Reload()
    {
        Unload();
        Launch();

        IO.Send("[SYS] Successfully reloaded Imperium.");
    }

    private static void OnLevelLoaded(bool isLoaded)
    {
        if (isLoaded) Settings.LoadAll();
    }

    private static void RegisterInterfaces()
    {
        Interface.OpenInterface.OnUpdate += openInterface =>
        {
            if (openInterface) PositionIndicator.Deactivate();
        };

        Interface.RegisterInterface<ImperiumUI>(
            ImpAssets.ImperiumUIObject,
            "ImperiumUI",
            "Imperium UI",
            "Imperium's main interface.",
            InputBindings.InterfaceMap.ImperiumUI
        );
        Interface.RegisterInterface<SpawningUI>(
            ImpAssets.SpawningUIObject,
            "SpawningUI",
            "Spawning",
            "Allows you to spawn objects\nsuch as Scrap or Entities.",
            InputBindings.InterfaceMap.SpawningUI,
            canOpenBindings: IsGameLevel
        );
        Interface.RegisterInterface<MapUI>(
            ImpAssets.MapUIObject,
            "MapUI",
            "Map",
            "Imperium's built-in map.",
            InputBindings.InterfaceMap.MapUI,
            canOpenBindings: IsGameLevel
        );
        Interface.RegisterInterface<MinimapSettings>(ImpAssets.MinimapSettingsObject);

        Interface.RefreshTheme();

        IO.LogInfo("[SYS] Imperium interfaces have been registered! \\o/");
    }

    private static void PreLaunchPatches()
    {
        Harmony.PatchAll(typeof(PreInitPatches));
    }
}