using BepInEx.Configuration;
using Imperium.Util;
using Librarium;
using Librarium.Binding;

namespace Imperium.Core.Settings;

internal class VisualizationSettings(ConfigFile config, IBinding<bool> isSceneLoaded, IBinding<bool> isEnabled)
    : SettingsContainer(config)
{
    /// <summary>
    ///     Visualization Settings
    /// </summary>
    internal readonly ImpConfig<bool> SmoothAnimations = new(
        config,
        "Visualization.Visualizers",
        "SmoothAnimations",
        true
    );

    internal readonly ImpConfig<bool> SSAlwaysOnTop = new(
        config,
        "Visualization.Insights",
        "AlwaysOnTop",
        true
    );

    internal readonly ImpConfig<bool> SSAutoScale = new(
        config,
        "Visualization.Insights",
        "AutoScale",
        true
    );

    internal readonly ImpConfig<bool> SSHideInactive = new(
        config,
        "Visualization.Insights",
        "HideInactive",
        false
    );

    internal readonly ImpConfig<float> SSOverlayScale = new(
        config,
        "Visualization.Insights",
        "OverlayScale",
        1
    );

    internal readonly ImpConfig<bool> NavMeshSurfaces = new(
        config,
        "Visualization.Overlays",
        "NavMeshSurfaces",
        false
    );

    /// <summary>
    ///     Gizmos
    /// </summary>
    internal readonly ImpConfig<bool> NoiseIndicators = new(
        config,
        "Visualization.Gizmos",
        "NoiseIndicators",
        false
    );

    internal readonly ImpConfig<bool> PlayerProximity = new(
        config,
        "Visualization.Gizmos",
        "PlayerProximity",
        false
    );

    internal readonly ImpConfig<bool> LevelPoints = new(
        config,
        "Visualization.Gizmos",
        "LevelPoints",
        false
    );

    internal readonly ImpConfig<bool> ValuableSpawns = new(
        config,
        "Visualization.Gizmos",
        "ValuableSpawns",
        false,
        primaryUpdate: value => Imperium.Visualization.StaticVisualizers.Point<ValuableVolume>(value, 0.2f, ImpAssets.FresnelBlue, overrideInactive: true)
    );
}