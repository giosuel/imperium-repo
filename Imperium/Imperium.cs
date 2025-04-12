#region

using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using Imperium.Core;
using Imperium.Core.EventLogging;
using Imperium.Core.Input;
using Imperium.Core.Lifecycle;
using Imperium.Core.Scripts;
using Imperium.Integration;
using Imperium.Interface.ImperiumUI;
using Imperium.Interface.MapUI;
using Imperium.Interface.SpawningUI;
using Imperium.Netcode;
using Imperium.Patches.Systems;
using Imperium.Util;
using Librarium.Binding;
using UnityEngine;
using UnityEngine.SceneManagement;

#endregion

namespace Imperium;

[BepInDependency("com.rune580.reposteamnetworking")]
[BepInDependency("REPOLib")]
[BepInPlugin(PLUGIN_GUID, PLUGIN_NAME, PLUGIN_VERSION)]
public class Imperium : BaseUnityPlugin
{
    public const string PLUGIN_GUID = "giosuel.Imperium";
    public const string PLUGIN_NAME = "Imperium";
    public const string PLUGIN_VERSION = "0.0.1";

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

    /*
     * Lifecycle systems. Instantiated when Imperium is launched (Stage 2).
     */
    internal static Core.Lifecycle.GameManager GameManager { get; private set; }
    internal static ObjectManager ObjectManager { get; private set; }
    internal static PlayerManager PlayerManager { get; private set; }
    internal static MoonManager MoonManager { get; private set; }
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
    internal static ImpTapeMeasure ImpTapeMeasure { get; private set; }
    internal static ImpInputBindings InputBindings { get; private set; }
    internal static ImpPositionIndicator ImpPositionIndicator { get; private set; }
    internal static ImpInterfaceManager Interface { get; private set; }
    internal static WaypointManager WaypointManager { get; private set; }

    /// <summary>
    ///     Set to true, then Imperium is initialized (Stage 1).
    /// </summary>
    internal static bool IsImperiumInitialized { get; private set; }

    /// <summary>
    ///     Set to true, then Imperium is launched (Stage 2) and ready to serve API calls.
    /// </summary>
    internal static bool IsImperiumLaunched { get; private set; }

    /// <summary>
    ///     Set to true, when Imperium is launched and imperium access is currently granted.
    /// </summary>
    internal static bool IsImperiumEnabled { get; private set; }

    /// <summary>
    ///     Binding that updates whenever the scene ship lands and takes off.
    /// </summary>
    internal static ImpBinaryBinding IsSceneLoaded { get; private set; }

    internal static ImpEvent SceneLoaded { get; private set; } = new();

    internal static GameObject GameObject { get; private set; }

    internal static ImpBinding<Camera> ActiveCamera;

    /// <summary>
    /// Imperium initialization (Stage 1)
    ///
    /// This happens as soon as BepInEx loads the Imperium plugin.
    /// </summary>
    private void Awake()
    {
        configFile = Config;
        Log = Logger;

        /*
         * Temporary settings instance for startup functionality.
         * This object will be re-instantiated once Imperium launches, meaning all listeners will be removed!
         */
        Settings = new ImpSettings(Config);

        IO = new ImpOutput(Log);
        StartupManager = new StartupManager();
        Networking = new ImpNetworking();

        if (!ImpAssets.Load()) return;

        Harmony = new Harmony(PLUGIN_GUID);
        PreLaunchPatches();

        IsImperiumInitialized = true;
        IO.LogInfo("[INIT] Imperium has been successfully initialized \\o/");
    }

    /// <summary>
    /// Imperium launch (Stage 2)
    ///
    /// This is executed after Imperium access has been granted by the host.
    /// </summary>
    internal static void Launch()
    {
        if (!IsImperiumInitialized || IsImperiumLaunched) return;

        GameObject = new GameObject("Imperium");
        DontDestroyOnLoad(GameObject);

        // Register scene loaded binding
        SceneManager.sceneLoaded += (_, _) => SceneLoaded.Trigger();

        // Register camera update when scene is loaded
        ActiveCamera = new ImpBinding<Camera>(PlayerAvatar.instance.localCamera);
        SceneLoaded.onTrigger += () => ActiveCamera.Set(PlayerAvatar.instance.localCamera);

        InputBlocker = new InputBlocker();
        InputBindings = new ImpInputBindings();

        // Re-instantiate settings to get rid of existing bindings
        Settings = new ImpSettings(configFile);
        IO.BindNotificationSettings(Settings);
        Networking.BindAllowClients(Settings.Preferences.AllowClients);

        IsSceneLoaded = new ImpBinaryBinding(false);

        Interface = ImpInterfaceManager.Create(Settings.Preferences.Theme, GameObject.transform);
        EventLog = new ImpEventLog();

        GameManager = ImpLifecycleObject.Create<Core.Lifecycle.GameManager>(
            GameObject.transform, IsSceneLoaded, ImpNetworking.ConnectedPlayers
        );
        MoonManager = ImpLifecycleObject.Create<MoonManager>(
            GameObject.transform, IsSceneLoaded, ImpNetworking.ConnectedPlayers
        );
        ArenaManager = ImpLifecycleObject.Create<ArenaManager>(
            GameObject.transform, IsSceneLoaded, ImpNetworking.ConnectedPlayers
        );
        ObjectManager = ImpLifecycleObject.Create<ObjectManager>(
            GameObject.transform, IsSceneLoaded, ImpNetworking.ConnectedPlayers
        );
        PlayerManager = ImpLifecycleObject.Create<PlayerManager>(
            GameObject.transform, IsSceneLoaded, ImpNetworking.ConnectedPlayers
        );
        WaypointManager = ImpLifecycleObject.Create<WaypointManager>(
            GameObject.transform, IsSceneLoaded, ImpNetworking.ConnectedPlayers
        );

        Visualization = new Visualization(GameObject.transform, ObjectManager, configFile);

        Map = ImpScript.Create<ImpMap>(GameObject.transform);
        Freecam = ImpScript.Create<ImpFreecam>(GameObject.transform);
        NightVision = ImpScript.Create<ImpNightVision>(GameObject.transform);
        ImpTapeMeasure = ImpScript.Create<ImpTapeMeasure>(
            GameObject.transform, ImpAssets.TapeIndicatorObject
        );
        ImpPositionIndicator = ImpScript.Create<ImpPositionIndicator>(
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

            // #if DEBUG
            // This needs to be here as it depends on the UI
            // ImpLevelEditor = ImpLevelEditor.Create();
            // #endif

            // Send scene update to ensure consistency in the UIs
            IsSceneLoaded.SetFalse();
        }
        else
        {
            DisableImperium();
        }
    }

    internal static void DisableImperium()
    {
        if (!IsImperiumEnabled) return;
        IsImperiumEnabled = false;

        Interface.Destroy();
        PlayerManager.IsFlying.SetFalse();
        Freecam.IsFreecamEnabled.SetFalse();

        InputBindings.BaseMap.Disable();
        InputBindings.StaticMap.Disable();
        InputBindings.FreecamMap.Disable();
        InputBindings.InterfaceMap.Disable();
    }

    internal static void EnableImperium()
    {
        if (!IsImperiumLaunched) return;
        IsImperiumEnabled = true;

        Settings.LoadAll();

        RegisterInterfaces();
        PlayerManager.UpdateCameras();

        InputBindings.BaseMap.Enable();
        InputBindings.StaticMap.Enable();
        InputBindings.FreecamMap.Enable();
        InputBindings.InterfaceMap.Enable();
    }

    internal static void Unload()
    {
        if (!IsImperiumLaunched) return;

        Harmony.UnpatchSelf();

        DisableImperium();

        Networking.Unsubscribe();

        IsImperiumLaunched = false;

        PreLaunchPatches();
    }

    internal static void Reload()
    {
        Unload();
        Launch();

        IO.Send("[SYS] Successfully reloaded Imperium.");
    }

    private static void RegisterInterfaces()
    {
        Interface.OpenInterface.onUpdate += openInterface =>
        {
            if (openInterface) ImpPositionIndicator.Deactivate();
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
            InputBindings.InterfaceMap.SpawningUI
        );
        Interface.RegisterInterface<MapUI>(
            ImpAssets.MapUIObject,
            "MapUI",
            "Map",
            "Imperium's built-in map.",
            InputBindings.InterfaceMap.MapUI
        );
        Interface.RegisterInterface<MinimapSettings>(ImpAssets.MinimapSettingsObject);
        // Interface.RegisterInterface<ComponentManager>(ImpAssets.ComponentManagerObject);

        Interface.RefreshTheme();

        IO.LogInfo("[SYS] Imperium interfaces have been registered! \\o/");
    }

    private static void PreLaunchPatches()
    {
        Harmony.PatchAll(typeof(PlayerAvatarPatch));
    }
}