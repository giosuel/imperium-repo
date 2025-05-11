#region

using BepInEx.Configuration;
using Librarium;
using Librarium.Binding;

#endregion

namespace Imperium.Core.Settings;

internal class WaypointSettings(ConfigFile config) : SettingsContainer(config)
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