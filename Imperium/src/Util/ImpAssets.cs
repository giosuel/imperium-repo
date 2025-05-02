#region

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEngine;

#endregion

namespace Imperium.Util;

internal abstract class ImpAssets
{
    /*
     * UI Prefabs
     */
    internal static GameObject ImperiumDockObject;
    internal static GameObject ImperiumTooltipObject;

    internal static GameObject ImperiumUIObject;
    internal static GameObject MapUIObject;
    internal static GameObject SpawningUIObject;

    internal static GameObject LayerSelectorObject;
    internal static GameObject ComponentManagerObject;
    internal static GameObject MinimapOverlayObject;
    internal static GameObject MinimapSettingsObject;

    internal static GameObject ControlCenterWindowObject;
    internal static GameObject ObjectExplorerWindowObject;
    internal static GameObject InfoWindowObject;
    internal static GameObject GrabberControlWindowObject;
    internal static GameObject EventLogWindowObject;
    internal static GameObject ArenaControlWindowObject;
    internal static GameObject RenderingWindowObject;
    internal static GameObject SaveEditorWindowObject;
    internal static GameObject TeleportationWindowObject;
    internal static GameObject VisualizationWindowObject;
    internal static GameObject PreferencesWindowObject;

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
     * Other Prefabs
     */
    internal static GameObject PositionIndicatorObject;
    internal static GameObject TapeIndicatorObject;
    internal static GameObject WaypointOverlay;
    internal static GameObject ObjectInsightPanel;
    internal static GameObject WaypointBeacon;
    internal static GameObject LevelPoint;

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
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/imperium_dock.prefab", out ImperiumDockObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/tooltip.prefab", out ImperiumTooltipObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/imperium_ui.prefab", out ImperiumUIObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/layer_selector.prefab", out LayerSelectorObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/component_manager.prefab", out ComponentManagerObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/map_ui.prefab", out MapUIObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/minimap.prefab", out MinimapOverlayObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/minimap_settings.prefab", out MinimapSettingsObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/spawning_ui.prefab", out SpawningUIObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/Windows/control_center.prefab", out ControlCenterWindowObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/Windows/info.prefab", out InfoWindowObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/Windows/grabber_control.prefab", out GrabberControlWindowObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/Windows/event_log.prefab", out EventLogWindowObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/Windows/object_explorer.prefab", out ObjectExplorerWindowObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/Windows/preferences.prefab", out PreferencesWindowObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/Windows/rendering.prefab", out RenderingWindowObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/Windows/save_editor.prefab", out SaveEditorWindowObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/Windows/arena_control.prefab", out ArenaControlWindowObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/Windows/teleportation.prefab", out TeleportationWindowObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/UI/Windows/visualization.prefab", out VisualizationWindowObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/tape_indicator.prefab", out TapeIndicatorObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/position_indicator.prefab", out PositionIndicatorObject),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/insight_panel.prefab", out ObjectInsightPanel),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/waypoint_beacon.prefab", out WaypointBeacon),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/level_point.prefab", out LevelPoint),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Prefabs/waypoint_overlay.prefab", out WaypointOverlay),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/xray.mat", out XRay),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/fresnel_white.mat", out FresnelWhite),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/fresnel_blue.mat", out FresnelBlue),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/fresnel_red.mat", out FresnelRed),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/fresnel_orange.mat", out FresnelOrange),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/fresnel_green.mat", out FresnelGreen),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/fresnel_yellow.mat", out FresnelYellow),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/wireframe_purple.mat", out WireframePurple),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/wireframe_orange.mat", out WireframeOrange),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/wireframe_cyan.mat", out WireframeCyan),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/wireframe_amaranth.mat", out WireframeAmaranth),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/wireframe_yellow.mat", out WireframeYellow),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/wireframe_green.mat", out WireframeGreen),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/wireframe_red.mat", out WireframeRed),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/shig.mat", out ShiggyMaterial),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Materials/navmesh.mat", out NavmeshMaterial),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Audio/ButtonClick.wav", out ButtonClick),
            LoadAsset(ImperiumAssets, "Assets/Imperium/Audio/OpenClick.ogg", out OpenClick)
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
        loadedObject = assets.LoadAsset<T>(path);
        if (!loadedObject)
        {
            Imperium.IO.LogError($"[INIT] Object '{path}' missing from the embedded Imperium asset bundle.");
            return false;
        }

        logBuffer.Add($"> Successfully loaded {path.Split("/").Last()} from asset bundle.");

        return true;
    }

    private static Stream LoadResource(string name) => Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
}