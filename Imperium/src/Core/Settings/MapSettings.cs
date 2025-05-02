#region

using BepInEx.Configuration;
using Librarium;
using Librarium.Binding;
using UnityEngine;

#endregion

namespace Imperium.Core.Settings;

internal class MapSettings(ConfigFile config, IBinding<bool> isSceneLoaded, IBinding<bool> isEnabled)
    : SettingsContainer(config)
{
    internal readonly ImpConfig<bool> MinimapEnabled = new(
        config,
        "Preferences.Map",
        "Minimap",
        true
    );

    internal readonly ImpConfig<bool> CompassEnabled = new(
        config,
        "Preferences.Map",
        "Compass",
        true
    );

    internal readonly ImpConfig<bool> RotationLock = new(
        config,
        "Preferences.Map",
        "RotationLock",
        true
    );

    internal readonly ImpConfig<bool> UnlockView = new(
        config,
        "Preferences.Map",
        "UnlockView",
        false
    );

    internal readonly ImpConfig<int> CameraLayerMask = new(
        config,
        "Preferences.Map",
        "LayerMask",
        ~LayerMask.GetMask("DirtFinderMap")
    );

    internal readonly ImpConfig<float> CameraZoom = new(
        config,
        "Preferences.Map",
        "Zoom",
        ImpConstants.DefaultMapCameraScale
    );

    internal readonly ImpConfig<bool> AutoClipping = new(
        config,
        "Preferences.Map",
        "AutoClipping",
        true
    );

    internal readonly ImpConfig<float> MinimapScale = new(
        config,
        "Preferences.Minimap",
        "Scale",
        1
    );

    internal readonly ImpConfig<bool> MinimapInfoPanel = new(
        config,
        "Preferences.Minimap",
        "InfoPanel",
        true
    );

    internal readonly ImpConfig<bool> MinimapLocationPanel = new(
        config,
        "Preferences.Minimap",
        "LocationPanel",
        true
    );
}