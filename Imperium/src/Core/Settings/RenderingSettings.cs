using BepInEx.Configuration;
using Imperium.Core.Lifecycle;
using Librarium;
using Librarium.Binding;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

namespace Imperium.Core.Settings;

internal class RenderingSettings(ConfigFile config, IBinding<bool> isSceneLoaded, IBinding<bool> isEnabled)
    : SettingsContainer(config)
{
    internal readonly ImpConfig<float> ResolutionMultiplier = new(
        config,
        "Rendering.General",
        "ResolutionMultiplier",
        1,
        primaryUpdate: _ => PlayerManager.UpdateCameras(),
        ignoreRefresh: true
    );

    internal readonly ImpConfig<float> FogStart = new(
        config,
        "Rendering.General",
        "FogStart",
        -1,
        primaryUpdate: value =>
        {
            RenderSettings.fogStartDistance = value < 0
                ? LevelGenerator.Instance.Level.FogStartDistance
                : value;
        }
    );

    internal readonly ImpConfig<float> FogEnd = new(
        config,
        "Rendering.General",
        "FogEnd",
        -1,
        primaryUpdate: value =>
        {
            RenderSettings.fogEndDistance = value < 0
                ? LevelGenerator.Instance.Level.FogEndDistance
                : value;

            EnvironmentDirector.Instance.MainCamera.farClipPlane = value < 0
                ? LevelGenerator.Instance.Level.FogEndDistance + 1f
                : value + 1f;
        }
    );

    internal readonly ImpConfig<bool> PostMain = new(
        config,
        "Rendering.PostProcessing",
        "Main",
        true,
        primaryUpdate: value => Imperium.ObjectManager.ToggleObject("Post Processing Main", value)
    );

    internal readonly ImpConfig<bool> PostDarkness = new(
        config,
        "Rendering.PostProcessing",
        "Darkness",
        true,
        primaryUpdate: value => PlayerAvatar.instance.localCamera.GetComponent<PostProcessLayer>().enabled = value
    );

    internal readonly ImpConfig<bool> PostOverlay = new(
        config,
        "Rendering.PostProcessing",
        "Overlay",
        true,
        primaryUpdate: value => Imperium.ObjectManager.ToggleObject("Post Processing Overlay", value)
    );

    internal readonly ImpConfig<bool> AvatarInMain = new(
        config,
        "Rendering.PlayerAvatar",
        "MainCamera",
        false,
        primaryUpdate: PlayerManager.ToggleLocalAvatar
    );

    internal readonly ImpConfig<bool> AvatarInFreecam = new(
        config,
        "Rendering.PlayerAvatar",
        "Freecam",
        true
    );
}