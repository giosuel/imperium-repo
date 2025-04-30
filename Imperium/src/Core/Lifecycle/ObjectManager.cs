#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Imperium.API.Types.Networking;
using Imperium.Core.Scripts;
using Imperium.Extensions;
using Imperium.Networking;
using Imperium.Types;
using Imperium.Util;
using Librarium.Binding;
using Photon.Pun;
using REPOLib.Extensions;
using REPOLib.Modules;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

#endregion

namespace Imperium.Core.Lifecycle;

/// <summary>
/// Lifecycle object that manages all object-related functionality. Keeps track of loaded and currently active objects.
/// </summary>
internal class ObjectManager : ImpLifecycleObject
{
    /*
     * Lists of globally loaded objects.
     *
     * These lists hold all the entities that can be spawned in Lethal Company, including the ones that are not in any
     * spawn list of any moon (e.g. Red Pill, Lasso Man).
     *
     * Loaded when Imperium initializes (Stage 1).
     */
    internal readonly ImpBinding<IReadOnlyList<Level>> LoadedLevels = new([]);
    internal readonly ImpBinding<IReadOnlyList<Module>> LoadedModules = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<Item>> LoadedItems = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<ValuableObject>> LoadedValuables = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<ExtendedEnemySetup>> LoadedEntities = new([]);

    // internal readonly ImpBinding<IReadOnlyCollection<Item>> LoadedScrap = new([]);
    // internal readonly ImpBinding<IReadOnlyDictionary<string, GameObject>> LoadedMapHazards = new();

    // Lists of bjects with network behaviours (e.g. clipboard, body, company cruiser)
    internal readonly ImpBinding<IReadOnlyDictionary<string, GameObject>> LoadedStaticPrefabs = new();

    // Lists of bjects without network behaviours (e.g. trees, vain shrouds, rocks)
    internal readonly ImpBinding<IReadOnlyDictionary<string, GameObject>> LoadedLocalStaticPrefabs = new();

    /*
     * Lists of objects loaded in the current scene.
     *
     * These lists hold the currently existing objects in the scene.
     * These are used by the object list in Imperium UI and is always up-to-date but
     * CAN CONTAIN NULL elements that have been marked for but not yet deleted during the last refresh.
     * Always ensure to check for null values before using the values in these lists.
     *
     * Loaded when Imperium launches (Stage 2).
     */
    internal readonly ImpBinding<IReadOnlyCollection<PlayerAvatar>> CurrentPlayers = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<EnemyParent>> CurrentLevelEntities = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<ItemAttributes>> CurrentLevelItems = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<ValuableObject>> CurrentLevelValuables = new([]);
    internal readonly ImpBinding<IReadOnlyCollection<ExtractionPoint>> CurrentLevelExtractionPoints = new([]);

    /*
     * Lists of local objects that don't have a network object or script to reference
     */
    // internal readonly ImpBinding<IReadOnlyCollection<GameObject>> CurrentLevelOutsideObjects = new([]);

    // Event that signalizes a change in any of the object lists
    internal event Action CurrentLevelObjectsChanged;

    /*
     * Misc scene objects.
     */
    // internal readonly ImpBinding<IReadOnlyCollection<RandomScrapSpawn>> CurrentScrapSpawnPoints = new([]);

    /*
     * Cache of game objects indexed by name for visualizers and other object access.
     *
     * Cleared when the ship is landing / taking off.
     */
    private readonly Dictionary<string, GameObject> ObjectCache = new();

    /*
     * List of Network IDs of disabled objects. Used to sync object active status over the network.
     */
    internal readonly ImpNetworkBinding<HashSet<int>> DisabledObjects = new(
        "DisabledObjects", Imperium.Networking, []
    );

    // Used by the server to execute a despawn request from a client via network ID
    private readonly Dictionary<int, GameObject> CurrentLevelObjects = [];

    private readonly ImpNetMessage<EntitySpawnRequest> entitySpawnMessage = new("SpawnEntity", Imperium.Networking);
    private readonly ImpNetMessage<ItemSpawnRequest> itemSpawnMessage = new("SpawnItem", Imperium.Networking);
    private readonly ImpNetMessage<ValuableSpawnRequest> valuableSpawnMessage = new("SpawnValuable", Imperium.Networking);

    private readonly ImpNetMessage<ObjectTeleportRequest> objectTeleportationRequest = new(
        "TeleportObject", Imperium.Networking
    );

    private readonly ImpNetMessage<ObjectDespawnRequest> objectDespawnMessage = new("DespawnObject", Imperium.Networking);

    private readonly ImpNetMessage<ExtractionCompleteRequest> extractionCompleteRequest = new(
        "ExtractionComplete", Imperium.Networking
    );

    private readonly ImpNetEvent objectsChangedEvent = new("ObjectsChanged", Imperium.Networking);

    private readonly HashSet<int> SpawnedEntityIds = [];

    protected override void Init()
    {
        FetchGlobalSpawnLists();
        FetchPlayers();

        RefreshLevelObjects();

        Imperium.IsArenaLoaded.onTrigger += RefreshLevelObjects;
        Imperium.IsArenaLoaded.onTrigger += FetchPlayers;

        objectsChangedEvent.OnClientRecive += RefreshLevelObjects;
        objectTeleportationRequest.OnClientRecive += OnObjectTeleportRequestClient;

        if (PhotonNetwork.IsMasterClient)
        {
            entitySpawnMessage.OnServerReceive += OnSpawnEntity;
            itemSpawnMessage.OnServerReceive += OnSpawnItem;
            valuableSpawnMessage.OnServerReceive += OnSpawnValuable;

            objectDespawnMessage.OnServerReceive += OnDespawnObject;
            objectTeleportationRequest.OnServerReceive += OnObjectTeleportRequestServer;

            extractionCompleteRequest.OnServerReceive += OnExtractionComplete;
        }
    }

    protected override void OnSceneLoad() => RefreshLevelObjects();

    protected override void OnPlayersUpdate(int playersConnected) => FetchPlayers();

    [ImpAttributes.RemoteMethod]
    internal void SpawnEntity(EntitySpawnRequest request) => entitySpawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void SpawnItem(ItemSpawnRequest request) => itemSpawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void SpawnValuable(ValuableSpawnRequest request) => valuableSpawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void DespawnObject(ObjectDespawnRequest request) => objectDespawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void TeleportObject(ObjectTeleportRequest request) => objectTeleportationRequest.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void CompleteExtraction(ExtractionCompleteRequest request)
    {
        extractionCompleteRequest.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void InvokeObjectsChanged() => objectsChangedEvent.DispatchToClients();

    private GameObject FindObject(string objName)
    {
        if (ObjectCache.TryGetValue(objName, out var obj) && obj) return obj;
        obj = GameObject.Find(objName);
        if (!obj) return null;
        ObjectCache[objName] = obj;
        return obj;
    }

    internal void ToggleObject(string objName, bool isOn)
    {
        var obj = FindObject(objName);
        if (obj) obj.SetActive(isOn);
    }

    private void FetchGlobalSpawnLists()
    {
        var allEntities = new HashSet<ExtendedEnemySetup>();
        var loadedEntityNames = new HashSet<string>();

        foreach (var enemyType in EnemyDirector.instance.GetEnemies())
        {
            if (enemyType.spawnObjects.Count < 1) continue;
            var parent = enemyType.spawnObjects[0].GetComponent<EnemyParent>();
            if (!parent || loadedEntityNames.Contains(parent.enemyName)) continue;

            allEntities.Add(new ExtendedEnemySetup
            {
                Enemy = enemyType,
                Difficulty = parent.difficulty,
                EnemyName = parent.enemyName,
                SpawnedTimeMax = parent.SpawnedTimeMax,
                SpawnedTimeMin = parent.SpawnedTimeMin,
                DespawnedTimeMax = parent.DespawnedTimeMax,
                DespawnedTimeMin = parent.DespawnedTimeMin,
            });

            loadedEntityNames.Add(parent.enemyName);
        }

        var allItems = Resources.FindObjectsOfTypeAll<Item>().ToHashSet();
        var allLevels = Resources.FindObjectsOfTypeAll<Level>()
            .Where(level => !ImpConstants.LevelBlacklist.Contains(level.NarrativeName))
            .ToList();
        var allModules = Resources.FindObjectsOfTypeAll<Module>().ToList();

        var allValuables = Valuables.GetValuables().Select(obj => obj.GetComponent<ValuableObject>()).ToHashSet();

        LoadedItems.Set(allItems);
        LoadedLevels.Set(allLevels);
        LoadedModules.Set(allModules);
        LoadedEntities.Set(allEntities);
        LoadedValuables.Set(allValuables);
    }

    internal void RefreshLevelObjects()
    {
        HashSet<EnemyParent> currentLevelEntities = [];
        HashSet<ItemAttributes> currentLevelItems = [];
        HashSet<ValuableObject> currentLevelValuables = [];
        HashSet<ExtractionPoint> currentLevelExtractionPoints = [];

        foreach (var enemy in FindObjectsByType<EnemyParent>(FindObjectsSortMode.None))
        {
            currentLevelEntities.Add(enemy);
            CurrentLevelObjects[enemy.photonView.ViewID] = enemy.gameObject;
        }

        foreach (var item in FindObjectsByType<ItemAttributes>(FindObjectsSortMode.None))
        {
            currentLevelItems.Add(item);
            CurrentLevelObjects[item.photonView.ViewID] = item.gameObject;
        }

        foreach (var valuable in FindObjectsByType<ValuableObject>(FindObjectsSortMode.None))
        {
            currentLevelValuables.Add(valuable);
            CurrentLevelObjects[valuable.photonView.ViewID] = valuable.gameObject;
        }

        foreach (var extractionPoint in FindObjectsByType<ExtractionPoint>(FindObjectsSortMode.None))
        {
            currentLevelExtractionPoints.Add(extractionPoint);
            CurrentLevelObjects[extractionPoint.photonView.ViewID] = extractionPoint.gameObject;
        }

        CurrentLevelItems.Set(currentLevelItems);
        CurrentLevelEntities.Set(currentLevelEntities);
        CurrentLevelValuables.Set(currentLevelValuables);
        CurrentLevelExtractionPoints.Set(currentLevelExtractionPoints);

        CurrentLevelObjectsChanged?.Invoke();
    }

    private void FetchPlayers()
    {
        CurrentPlayers.Set(GameDirector.instance.PlayerList.ToHashSet());
    }

    #region RPC Handlers

    [ImpAttributes.HostOnly]
    private void OnSpawnEntity(EntitySpawnRequest request, ulong clientId)
    {
        var spawningEntity = LoadedEntities.Value.FirstOrDefault(entity => entity.EnemyName == request.Name);
        var enemySetup = spawningEntity?.Enemy;

        if (!enemySetup)
        {
            Imperium.IO.LogError($"[SPAWN] [R] Unable to find requested entity '{request.Name}'.");
            return;
        }

        foreach (var spawnObject in enemySetup.GetSortedSpawnObjects())
        {
            if (spawnObject == null)
            {
                Imperium.IO.LogError($"[SPAWN] [R] Unable to spawn prefab for requested entity '{request.Name}'.");
                continue;
            }

            var prefabId = ResourcesHelper.GetEnemyPrefabPath(spawnObject);
            var obj = NetworkPrefabs.SpawnNetworkPrefab(prefabId, request.SpawnPosition, Quaternion.identity);
            if (obj == null || !obj.TryGetComponent(out EnemyParent enemyParent)) continue;

            enemyParent.SetupDone = true;

            var enemy = obj.GetComponentInChildren<Enemy>();
            if (enemy != null)
            {
                CurrentLevelObjects[enemy.photonView.ViewID] = obj;
                SpawnedEntityIds.Add(enemy.photonView.ViewID);
                enemy.EnemyTeleported(request.SpawnPosition);
            }
        }

        if (request.SendNotification)
        {
            var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
            var verbString = request.Amount == 1 ? "has" : "have";

            Imperium.Networking.SendLog(new NetworkNotification
            {
                Message = $"{mountString} loyal {request.Name} {verbString} been spawned!",
                Type = NotificationType.Spawning
            });
        }

        objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnSpawnItem(ItemSpawnRequest request, ulong clientId)
    {
        var spawningItem = LoadedItems.Value.FirstOrDefault(item => item.itemName == request.Name);

        if (!spawningItem)
        {
            Imperium.IO.LogError($"[SPAWN] [R] Unable to find requested item '{request.Name}'.");
            return;
        }

        var prefabId = ResourcesHelper.GetItemPrefabPath(spawningItem);

        for (var i = 0; i < request.Amount; i++)
        {
            var obj = NetworkPrefabs.SpawnNetworkPrefab(prefabId, request.SpawnPosition, Quaternion.identity);
            if (obj == null)
            {
                Imperium.IO.LogError($"[SPAWN] Failed to spawn item '{request.Name}'.");
                return;
            }

            SpawnedEntityIds.Add(obj.GetComponent<PhotonView>().ViewID);
        }

        var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
        var verbString = request.Amount == 1 ? "has" : "have";

        if (request.SendNotification)
        {
            Imperium.Networking.SendLog(new NetworkNotification
            {
                Message = $"{mountString} {request.Name} {verbString} been spawned!",
                Type = NotificationType.Spawning
            });
        }

        objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnSpawnValuable(ValuableSpawnRequest request, ulong clientId)
    {
        var spawningValuable = LoadedValuables.Value.FirstOrDefault(valuable => valuable.name == request.Name);

        if (!spawningValuable)
        {
            Imperium.IO.LogError($"[SPAWN] [R] Unable to find requested valuable '{request.Name}'.");
            return;
        }

        var prefabId = ResourcesHelper.GetValuablePrefabPath(spawningValuable);

        for (var i = 0; i < request.Amount; i++)
        {
            var obj = NetworkPrefabs.SpawnNetworkPrefab(prefabId, request.SpawnPosition, Quaternion.identity);
            if (obj == null)
            {
                Imperium.IO.LogError($"[SPAWN] Failed to spawn valuable '{request.Name}'.");
                return;
            }

            SpawnedEntityIds.Add(obj.GetComponent<PhotonView>().ViewID);
        }

        var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
        var verbString = request.Amount == 1 ? "has" : "have";

        if (request.SendNotification)
        {
            Imperium.Networking.SendLog(new NetworkNotification
            {
                Message = $"{mountString} {request.Name} {verbString} been spawned!",
                Type = NotificationType.Spawning
            });
        }

        objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnObjectTeleportRequestServer(ObjectTeleportRequest request, ulong clientId)
    {
        objectTeleportationRequest.DispatchToClients(request);
    }

    [ImpAttributes.LocalMethod]
    private void OnObjectTeleportRequestClient(ObjectTeleportRequest request)
    {
        // if (!CurrentLevelObjects.TryGetValue(request.NetworkId, out var obj) || !obj)
        // {
        //     Imperium.IO.LogError($"[NET] Failed to teleport object item with net ID {request.NetworkId}");
        //     return;
        // }
        //
        // if (obj.TryGetComponent<GrabbableObject>(out var item))
        // {
        //     var itemTransform = item.transform;
        //     itemTransform.position = request.Destination + Vector3.up;
        //     item.startFallingPosition = itemTransform.position;
        //     if (item.transform.parent)
        //     {
        //         item.startFallingPosition = item.transform.parent.InverseTransformPoint(item.startFallingPosition);
        //     }
        //
        //     item.FallToGround();
        //     item.PlayDropSFX();
        // }
        // else if (obj.TryGetComponent<Landmine>(out _))
        // {
        //     obj.transform.parent.position = request.Destination;
        // }
        // else
        // {
        //     obj.transform.position = request.Destination;
        // }
    }

    [ImpAttributes.HostOnly]
    private void OnExtractionComplete(ExtractionCompleteRequest request, ulong senderId)
    {
        var extractionPoint = CurrentLevelExtractionPoints.Value.FirstOrDefault(
            point => point.photonView.ViewID == request.ViewId
        );

        if (!extractionPoint)
        {
            Imperium.IO.LogInfo($"[OBJ] Failed to find extraction point to complete with ID '{request.ViewId}'");
            return;
        }

        extractionPoint.StateSet(ExtractionPoint.State.Complete);
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnObject(ObjectDespawnRequest request, ulong senderId)
    {
        if (!CurrentLevelObjects.TryGetValue(request.ViewId, out var obj))
        {
            Imperium.IO.LogError($"[SPAWN] [R] Failed to despawn object with view ID {request.ViewId}");
            return;
        }

        // If the object was an enemy, remove from spawned list
        if (obj.TryGetComponent<EnemyParent>(out var enemyParent))
        {
            EnemyDirector.instance.enemiesSpawned.Remove(enemyParent);
        }

        PhotonNetwork.Destroy(obj);
        objectsChangedEvent.DispatchToClients();
    }

    #endregion
}