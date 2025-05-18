#region

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

#endregion

namespace Imperium.Util;

internal static class ImpAssets
{
    /*
     * UI Prefabs
     */
    internal static GameObject ImperiumTooltipObject;

    internal static GameObject ImperiumUIObject;
    internal static GameObject MapUIObject;
    internal static GameObject ConsoleUIObject;

    internal static GameObject LayerSelectorObject;
    internal static GameObject ComponentManagerObject;
    internal static GameObject MinimapOverlayObject;
    internal static GameObject MinimapSettingsObject;

    internal static GameObject ControlCenterWindowObject;
    internal static GameObject ObjectExplorerWindowObject;
    internal static GameObject InfoWindowObject;
    internal static GameObject LevelGenerationWindowObject;
    internal static GameObject EventLogWindowObject;
    internal static GameObject GameControlWindowObject;
    internal static GameObject RenderingWindowObject;
    internal static GameObject SaveEditorWindowObject;
    internal static GameObject TeleportationWindowObject;
    internal static GameObject VisualizationWindowObject;
    internal static GameObject PreferencesWindowObject;
    internal static GameObject PortalWindowObject;
    internal static GameObject UpgradesWindowObject;

    /*
     * Materials
     */
    internal static Material FresnelWhite;
    internal static Material FresnelBlue;
    internal static Material FresnelYellow;
    internal static Material FresnelGreen;
    internal static Material FresnelRed;
    internal static Material FresnelOrange;
    internal static Material WireframePurple;
    internal static Material WireframeOrange;
    internal static Material WireframeCyan;
    internal static Material WireframeAmaranth;
    internal static Material WireframeYellow;
    internal static Material WireframeGreen;
    internal static Material WireframeRed;
    internal static Material XRay;
    internal static Material ShiggyMaterial;
    internal static Material NavmeshMaterial;

    /*
     * Icons
     */
    internal static Sprite IconCommandAction;
    internal static Sprite IconCommandReload;
    internal static Sprite IconCommandSpawn;
    internal static Sprite IconCommandWindow;
    internal static Sprite IconCommandSetting;
    internal static Sprite IconVisualizer;
    internal static Sprite IconKillPlayer;
    internal static Sprite IconRevivePlayer;
    internal static Sprite IconGrabber;

    /*
     * Window Icons
     */
    internal static Sprite IconControlCenter;
    internal static Sprite IconEventLog;
    internal static Sprite IconGameControl;
    internal static Sprite IconLevelGeneration;
    internal static Sprite IconObjectExplorer;
    internal static Sprite IconPortal;
    internal static Sprite IconPreferences;
    internal static Sprite IconRendering;
    internal static Sprite IconTeleportation;
    internal static Sprite IconUpgrades;
    internal static Sprite IconVisualizers;

    /*
     * Other Prefabs
     */
    internal static GameObject PositionIndicatorObject;
    internal static GameObject TapeIndicatorObject;
    internal static GameObject WaypointOverlay;
    internal static GameObject ObjectInsightPanel;
    internal static GameObject WaypointBeacon;
    internal static GameObject LevelPoint;
    internal static GameObject EnemyStatus;

    /*
     * Audio Clips
     */
    internal static AudioClip ButtonClick;
    internal static AudioClip OpenClick;

    internal static AssetBundle ImperiumAssets;

    internal static bool Load()
    {
        if (!LoadAssets())
        {
            Imperium.IO.LogInfo("[INIT] Failed to load one or more assets from assembly, aborting!");
            return false;
        }

        return true;
    }

    private static bool LoadAssets()
    {
        using (var assetBundleStream = LoadResource("Imperium.resources.assets.imperium_assets"))
        {
            ImperiumAssets = AssetBundle.LoadFromStream(assetBundleStream);
        }

        if (ImperiumAssets == null)
        {
            Imperium.IO.LogError("[INIT] Failed to load assets from assembly, aborting!");
            return false;
        }

        logBuffer = [];
        List<bool> loadResults =
        [
            LoadAsset(ImperiumAssets, "Prefabs/UI/tooltip.prefab", out ImperiumTooltipObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/imperium_ui.prefab", out ImperiumUIObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/layer_selector.prefab", out LayerSelectorObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/component_manager.prefab", out ComponentManagerObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/map_ui.prefab", out MapUIObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/minimap.prefab", out MinimapOverlayObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/minimap_settings.prefab", out MinimapSettingsObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/console_ui.prefab", out ConsoleUIObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/Windows/control_center.prefab", out ControlCenterWindowObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/Windows/info.prefab", out InfoWindowObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/Windows/level_generation.prefab", out LevelGenerationWindowObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/Windows/event_log.prefab", out EventLogWindowObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/Windows/object_explorer.prefab", out ObjectExplorerWindowObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/Windows/preferences.prefab", out PreferencesWindowObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/Windows/portal.prefab", out PortalWindowObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/Windows/upgrades.prefab", out UpgradesWindowObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/Windows/rendering.prefab", out RenderingWindowObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/Windows/save_editor.prefab", out SaveEditorWindowObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/Windows/game_control.prefab", out GameControlWindowObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/Windows/teleportation.prefab", out TeleportationWindowObject),
            LoadAsset(ImperiumAssets, "Prefabs/UI/Windows/visualization.prefab", out VisualizationWindowObject),
            LoadAsset(ImperiumAssets, "Prefabs/tape_indicator.prefab", out TapeIndicatorObject),
            LoadAsset(ImperiumAssets, "Prefabs/position_indicator.prefab", out PositionIndicatorObject),
            LoadAsset(ImperiumAssets, "Prefabs/insight_panel.prefab", out ObjectInsightPanel),
            LoadAsset(ImperiumAssets, "Prefabs/waypoint_beacon.prefab", out WaypointBeacon),
            LoadAsset(ImperiumAssets, "Prefabs/level_point.prefab", out LevelPoint),
            LoadAsset(ImperiumAssets, "Prefabs/enemy_status.prefab", out EnemyStatus),
            LoadAsset(ImperiumAssets, "Prefabs/waypoint_overlay.prefab", out WaypointOverlay),
            LoadAsset(ImperiumAssets, "Materials/xray.mat", out XRay),
            LoadAsset(ImperiumAssets, "Materials/fresnel_white.mat", out FresnelWhite),
            LoadAsset(ImperiumAssets, "Materials/fresnel_blue.mat", out FresnelBlue),
            LoadAsset(ImperiumAssets, "Materials/fresnel_red.mat", out FresnelRed),
            LoadAsset(ImperiumAssets, "Materials/fresnel_orange.mat", out FresnelOrange),
            LoadAsset(ImperiumAssets, "Materials/fresnel_green.mat", out FresnelGreen),
            LoadAsset(ImperiumAssets, "Materials/fresnel_yellow.mat", out FresnelYellow),
            LoadAsset(ImperiumAssets, "Materials/wireframe_purple.mat", out WireframePurple),
            LoadAsset(ImperiumAssets, "Materials/wireframe_orange.mat", out WireframeOrange),
            LoadAsset(ImperiumAssets, "Materials/wireframe_cyan.mat", out WireframeCyan),
            LoadAsset(ImperiumAssets, "Materials/wireframe_amaranth.mat", out WireframeAmaranth),
            LoadAsset(ImperiumAssets, "Materials/wireframe_yellow.mat", out WireframeYellow),
            LoadAsset(ImperiumAssets, "Materials/wireframe_green.mat", out WireframeGreen),
            LoadAsset(ImperiumAssets, "Materials/wireframe_red.mat", out WireframeRed),
            LoadAsset(ImperiumAssets, "Materials/shig.mat", out ShiggyMaterial),
            LoadAsset(ImperiumAssets, "Materials/navmesh.mat", out NavmeshMaterial),
            LoadAsset(ImperiumAssets, "Icons/visualizer.png", out IconVisualizer),
            LoadAsset(ImperiumAssets, "Icons/setting.png", out IconCommandSetting),
            LoadAsset(ImperiumAssets, "Icons/action.png", out IconCommandAction),
            LoadAsset(ImperiumAssets, "Icons/reload.png", out IconCommandReload),
            LoadAsset(ImperiumAssets, "Icons/spawn.png", out IconCommandSpawn),
            LoadAsset(ImperiumAssets, "Icons/window.png", out IconCommandWindow),
            LoadAsset(ImperiumAssets, "Icons/kill.png", out IconKillPlayer),
            LoadAsset(ImperiumAssets, "Icons/revive.png", out IconRevivePlayer),
            LoadAsset(ImperiumAssets, "Icons/grabber.png", out IconGrabber),
            LoadAsset(ImperiumAssets, "ImperiumSprites/control.png", out IconControlCenter),
            LoadAsset(ImperiumAssets, "ImperiumSprites/notebook.png", out IconEventLog),
            LoadAsset(ImperiumAssets, "ImperiumSprites/game.png", out IconGameControl),
            LoadAsset(ImperiumAssets, "ImperiumSprites/blueprint.png", out IconLevelGeneration),
            LoadAsset(ImperiumAssets, "ImperiumSprites/explorer.png", out IconObjectExplorer),
            LoadAsset(ImperiumAssets, "ImperiumSprites/portal.png", out IconPortal),
            LoadAsset(ImperiumAssets, "ImperiumSprites/wrench.png", out IconPreferences),
            LoadAsset(ImperiumAssets, "ImperiumSprites/camera.png", out IconRendering),
            LoadAsset(ImperiumAssets, "ImperiumSprites/teleport.png", out IconTeleportation),
            LoadAsset(ImperiumAssets, "ImperiumSprites/upgrade.png", out IconUpgrades),
            LoadAsset(ImperiumAssets, "ImperiumSprites/visualizer.png", out IconVisualizers),
            LoadAsset(ImperiumAssets, "Audio/ButtonClick.wav", out ButtonClick),
            LoadAsset(ImperiumAssets, "Audio/OpenClick.ogg", out OpenClick)
        ];

        Imperium.IO.LogBlock(logBuffer, "Imperium Resource Loader");

        return loadResults.All(result => result);
    }

    private static readonly Dictionary<string, Sprite> spriteCache = new();

    internal static Sprite LoadSpriteFromFiles(string spriteName)
    {
        var spritePath = new[]
            {
                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                "images",
                $"{spriteName}.png"
            }
            .Aggregate(Path.Combine);

        if (spriteCache.TryGetValue(spritePath, out var sprite)) return sprite;

        if (File.Exists(spritePath))
        {
            var fileData = File.ReadAllBytes(spritePath);
            var texture = new Texture2D(2, 2);
            if (texture.LoadImage(fileData))
            {
                sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0, 0));
                spriteCache[spriteName] = sprite;
                return sprite;
            }
        }

        return null;
    }

    private static List<string> logBuffer = [];

    private static bool LoadAsset<T>(AssetBundle assets, string path, out T loadedObject) where T : Object
    {
        loadedObject = assets.LoadAsset<T>($"Assets/Imperium/{path}");
        if (!loadedObject)
        {
            Imperium.IO.LogError(
                $"[INIT] Object 'Assets/Imperium/{path}' missing from the embedded Imperium asset bundle."
            );
            return false;
        }

        logBuffer.Add($"> Successfully loaded {path.Split("/").Last()} from asset bundle.");

        return true;
    }

    private static Stream LoadResource(string name) => Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
}