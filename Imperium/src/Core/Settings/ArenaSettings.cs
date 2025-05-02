#region

using BepInEx.Configuration;
using Imperium.Util;
using Librarium;
using Librarium.Binding;

#endregion

namespace Imperium.Core.Settings;

internal class ArenaSettings(ConfigFile config, IBinding<bool> isSceneLoaded, IBinding<bool> isEnabled)
    : SettingsContainer(config)
{
    [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> DisableGameOver = new(
        config,
        "Game.Arena",
        "DisableGameOver",
        false
    );

    [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> DisableEnemies = new(
        config,
        "Game.Arena",
        "DisableEnemies",
        false
    );

    [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> SimulateLag = new(
        config,
        "Game.Arena",
        "SimulateLag",
        false
    );

    [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> SpawnClose = new(
        config,
        "Game.Arena.Enemies",
        "SpawnClose",
        false
    );

    [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> DespawnClose = new(
        config,
        "Game.Arena.Enemies",
        "DespawnClose",
        false
    );

    [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> DisableVision = new(
        config,
        "Game.Arena.Enemies",
        "DisableVision",
        false
    );

    [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> ShortAction = new(
        config,
        "Game.Arena.Enemies",
        "ShortAction",
        false
    );

    [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<float> SpawnTimer = new(
        config,
        "Game.Arena.Enemies",
        "SpawnTimer",
        -1
    );

    [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<float> DespawnTimer = new(
        config,
        "Game.Arena.Enemies",
        "DespawnTimer",
        -1
    );

    [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> EnemyGrabInfiniteDuration = new(
        config,
        "Game.Arena.Enemies",
        "GrabInfiniteDuration",
        false
    );

    [ImpAttributes.HostMasterBinding] internal readonly ImpConfig<bool> EnemyGrabInfiniteStrength = new(
        config,
        "Game.Arena.Enemies",
        "GrabInfiniteStrength",
        false
    );
}