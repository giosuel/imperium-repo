using BepInEx.Configuration;
using Librarium;
using Librarium.Binding;

namespace Imperium.Core.Settings;

internal class WaypointSettings(ConfigFile config, IBinding<bool> isSceneLoaded, IBinding<bool> isEnabled)
    : SettingsContainer(config)
{
    internal readonly ImpConfig<bool> EnableBeacons = new(
        config,
        "Waypoints",
        "EnableBeacons",
        true
    );

    internal readonly ImpConfig<bool> EnableOverlay = new(
        config,
        "Waypoints",
        "EnableOverlay",
        true
    );
}