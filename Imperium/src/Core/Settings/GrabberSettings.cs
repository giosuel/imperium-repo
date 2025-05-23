#region

using BepInEx.Configuration;
using Librarium;
using Librarium.Binding;

#endregion

namespace Imperium.Core.Settings;

internal class GrabberSettings(ConfigFile config) : SettingsContainer(config)
{
    internal readonly ImpConfig<bool> StickyGrabber = new(
        config,
        "Grabber",
        "StickyGrabber",
        false,
        primaryUpdate: value => PhysGrabber.instance.debugStickyGrabber = value
    );

    internal readonly ImpConfig<float> GrabStrength = new(
        config,
        "Grabber",
        "GrabStrength",
        ImpConstants.DefaultGrabStrength,
        primaryUpdate: value =>
        {
            PhysGrabber.instance.grabStrength =
                value + StatsManager.instance.playerUpgradeStrength[Imperium.Player.steamID] * 0.2f;
        }
    );

    internal readonly ImpConfig<float> ThrowStrength = new(
        config,
        "Grabber",
        "ThrowStrength",
        ImpConstants.DefaultThrowStrength,
        primaryUpdate: value =>
        {
            PhysGrabber.instance.throwStrength =
                value + StatsManager.instance.playerUpgradeThrow[Imperium.Player.steamID] * 0.3f;
        }
    );

    internal readonly ImpConfig<float> BaseRange = new(
        config,
        "Grabber",
        "BaseRange",
        ImpConstants.DefaultGrabberRange,
        primaryUpdate: value =>
        {
            PhysGrabber.instance.grabRange =
                value + StatsManager.instance.playerUpgradeRange[Imperium.Player.steamID] * 1;
        }
    );

    internal readonly ImpConfig<float> ReleaseDistance = new(
        config,
        "Grabber",
        "ReleaseDistance",
        ImpConstants.DefaultGrabberReleaseDistance,
        primaryUpdate: value => PhysGrabber.instance.grabReleaseDistance = value
    );

    internal readonly ImpConfig<float> MinDistance = new(
        config,
        "Grabber",
        "MinDistance",
        ImpConstants.DefaultGrabberMinDistance,
        primaryUpdate: value => PhysGrabber.instance.minDistanceFromPlayer = value
    );

    internal readonly ImpConfig<float> MaxDistance = new(
        config,
        "Grabber",
        "MaxDistance",
        ImpConstants.DefaultGrabberMaxDistance,
        primaryUpdate: value => PhysGrabber.instance.maxDistanceFromPlayer = value
    );

    internal readonly ImpConfig<float> SpringConstant = new(
        config,
        "Grabber",
        "SpringConstant",
        ImpConstants.DefaultGrabberSpringConstant,
        primaryUpdate: value => PhysGrabber.instance.springConstant = value
    );

    internal readonly ImpConfig<float> DampingConstant = new(
        config,
        "Grabber",
        "DampingConstant",
        ImpConstants.DefaultGrabberDampingConstant,
        primaryUpdate: value => PhysGrabber.instance.dampingConstant = value
    );

    internal readonly ImpConfig<float> ForceConstant = new(
        config,
        "Grabber",
        "ForceConstant",
        ImpConstants.DefaultGrabberForceConstant,
        primaryUpdate: value => PhysGrabber.instance.forceConstant = value
    );

    internal readonly ImpConfig<float> ForceMax = new(
        config,
        "Grabber",
        "ForceMax",
        ImpConstants.DefaultGrabberMaxForce,
        primaryUpdate: value => PhysGrabber.instance.forceConstant = value
    );
}