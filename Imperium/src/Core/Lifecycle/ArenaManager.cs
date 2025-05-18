#region

using Imperium.Networking;

#endregion

namespace Imperium.Core.Lifecycle;

internal class ArenaManager : ImpLifecycleObject
{
    protected override void Init()
    {
    }

    internal readonly ImpNetworkBinding<bool> DisableGameOver = new(
        "DisableGameOver",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Game.DisableGameOver
    );

    internal readonly ImpNetworkBinding<bool> DisableEnemies = new(
        "DisableEnemies",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Game.DisableEnemies,
        onUpdateClient: value => LevelGenerator.Instance.DebugNoEnemy = value
    );

    internal readonly ImpNetworkBinding<bool> SpawnClose = new(
        "EnemySpawnClose",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Game.SpawnClose,
        onUpdateClient: value => EnemyDirector.instance.debugSpawnClose = value
    );

    internal readonly ImpNetworkBinding<bool> DespawnClose = new(
        "EnemyDespawnClose",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Game.DespawnClose,
        onUpdateClient: value => EnemyDirector.instance.debugDespawnClose = value
    );

    internal readonly ImpNetworkBinding<bool> DisableVision = new(
        "EnemyDisableVision",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Game.DisableVision,
        onUpdateClient: value => EnemyDirector.instance.debugNoVision = value
    );

    internal readonly ImpNetworkBinding<bool> ShortAction = new(
        "EnemyShortAction",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Game.ShortAction,
        onUpdateClient: value => EnemyDirector.instance.debugShortActionTimer = value
    );

    internal readonly ImpNetworkBinding<float> SpawnTimer = new(
        "EnemySpawnTimer",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Game.SpawnTimer,
        onUpdateClient: value => EnemyDirector.instance.debugEnemyEnableTime = value
    );

    internal readonly ImpNetworkBinding<float> DespawnTimer = new(
        "EnemyDespawnTimer",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Game.DespawnTimer,
        onUpdateClient: value => EnemyDirector.instance.debugEnemyDisableTime = value
    );

    internal readonly ImpNetworkBinding<bool> EnemyGrabInfiniteDuration = new(
        "EnemyGrabInfiniteDuration",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Game.EnemyGrabInfiniteDuration,
        onUpdateClient: value => EnemyDirector.instance.debugNoGrabMaxTime = value
    );

    internal readonly ImpNetworkBinding<bool> EnemyGrabInfiniteStrength = new(
        "EnemyGrabInfiniteStrength",
        Imperium.Networking,
        masterBinding: Imperium.Settings.Game.EnemyGrabInfiniteStrength,
        onUpdateClient: value => EnemyDirector.instance.debugEasyGrab = value
    );
}