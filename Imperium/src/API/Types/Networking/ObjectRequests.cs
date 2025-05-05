// ReSharper disable Unity.RedundantAttributeOnTarget

#region

using UnityEngine;

#endregion

namespace Imperium.API.Types.Networking;

public readonly struct EntitySpawnRequest()
{
    [SerializeField] public string Name { get; init; }
    [SerializeField] public Vector3 SpawnPosition { get; init; } = default;
    [SerializeField] public int Amount { get; init; } = 1;
    [SerializeField] public int Health { get; init; } = -1;
    [SerializeField] public bool SendNotification { get; init; } = false;
}

public readonly struct ItemSpawnRequest()
{
    [SerializeField] public string Name { get; init; }
    [SerializeField] public Vector3 SpawnPosition { get; init; } = default;
    [SerializeField] public int Amount { get; init; } = 1;
    [SerializeField] public bool SendNotification { get; init; } = false;
}

public readonly struct ValuableSpawnRequest()
{
    [SerializeField] public string Name { get; init; }
    [SerializeField] public Vector3 SpawnPosition { get; init; } = default;
    [SerializeField] public int Amount { get; init; } = 1;
    [SerializeField] public int Value { get; init; } = -1;
    [SerializeField] public bool SendNotification { get; init; } = false;
}

public readonly struct ObjectDespawnRequest
{
    // ViewId in multiplayer and InstanceId in singleplayer
    [SerializeField] public int ObjectId { get; init; }
}

public readonly struct ExtractionCompleteRequest
{
    // ViewId in multiplayer and InstanceId in singleplayer
    [SerializeField] public int ObjectId { get; init; }
}

public readonly struct EnemyTeleportRequest
{
    // ViewId in multiplayer and InstanceId in singleplayer
    [SerializeField] public int ObjectId { get; init; }
    [SerializeField] public Vector3 Destination { get; init; }
}

public readonly struct ValuableTeleportRequest
{
    // ViewId in multiplayer and InstanceId in singleplayer
    [SerializeField] public int ObjectId { get; init; }
    [SerializeField] public Vector3 Destination { get; init; }
}

public readonly struct ItemTeleportRequest
{
    // ViewId in multiplayer and InstanceId in singleplayer
    [SerializeField] public int ObjectId { get; init; }
    [SerializeField] public Vector3 Destination { get; init; }
}

public enum ObjectType
{
    Player,
    Entity,
    Item,
    Valuable,
    ExtractionPoint
}