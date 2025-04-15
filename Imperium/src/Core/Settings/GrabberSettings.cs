using BepInEx.Configuration;
using Librarium;
using Librarium.Binding;

namespace Imperium.Core.Settings;

internal class GrabberSettings(ConfigFile config, IBinding<bool> isSceneLoaded, IBinding<bool> isEnabled)
    : SettingsContainer(config)
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
        primaryUpdate: value => PhysGrabber.instance.grabStrength = value
    );

    internal readonly ImpConfig<float> ThrowStrength = new(
        config,
        "Grabber",
        "GrabStrenThrowStrengthgth",
        ImpConstants.DefaultThrowStrength,
        primaryUpdate: value => PhysGrabber.instance.throwStrength = value
    );

    internal readonly ImpConfig<float> BaseRange = new(
        config,
        "Grabber",
        "BaseRange",
        ImpConstants.DefaultGrabberRange,
        primaryUpdate: value => PhysGrabber.instance.grabRange = value
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