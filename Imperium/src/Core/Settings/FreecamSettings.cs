#region

using BepInEx.Configuration;
using Librarium;
using Librarium.Binding;

#endregion

namespace Imperium.Core.Settings;

internal class FreecamSettings(ConfigFile config) : SettingsContainer(config)
{
    internal readonly ImpConfig<bool> LayerSelector = new(
        config,
        "Preferences.Freecam",
        "LayerSelector",
        true
    );

    internal readonly ImpConfig<int> FreecamLayerMask = new(
        config,
        "Preferences.Freecam",
        "LayerMask",
        // Layer mask includes everything except UI and the dirt finder map
        -4129
    );

    internal readonly ImpConfig<float> FreecamMovementSpeed = new(
        config,
        "Preferences.Freecam",
        "MovementSpeed",
        20
    );

    internal readonly ImpConfig<float> FreecamFieldOfView = new(
        config,
        "Preferences.Freecam",
        "FieldOfView",
        ImpConstants.DefaultFOV
    );
}