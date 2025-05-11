#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.API.Types.Networking;
using Imperium.Extensions;
using Imperium.Networking;
using Imperium.Types;
using Imperium.Util;
using Librarium.Binding;
using Photon.Pun;
using REPOLib.Modules;
using UnityEngine;

#endregion

namespace Imperium.Core.Lifecycle;

/// <summary>
///     Lifecycle object that manages all object-related functionality. Keeps track of loaded and currently active objects.
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

    // Used by the server to execute a despawn request from a client via View ID in multiplayer and Instance ID in singleplayer
    private readonly Dictionary<int, GameObject> CurrentLevelObjects = [];

    private readonly ImpNetMessage<EntitySpawnRequest> entitySpawnMessage = new("SpawnEntity", Imperium.Networking);
    private readonly ImpNetMessage<ItemSpawnRequest> itemSpawnMessage = new("SpawnItem", Imperium.Networking);
    private readonly ImpNetMessage<ValuableSpawnRequest> valuableSpawnMessage = new("SpawnValuable", Imperium.Networking);

    private readonly ImpNetMessage<EnemyTeleportRequest> enemyTeleportationRequest = new(
        "TeleportEnemy", Imperium.Networking
    );

    private readonly ImpNetMessage<ValuableTeleportRequest> valuableTeleportationRequest = new(
        "TeleportValuable", Imperium.Networking
    );

    private readonly ImpNetMessage<ItemTeleportRequest> itemTeleportationRequest = new(
        "TeleportItem", Imperium.Networking
    );

    private readonly ImpNetMessage<ObjectDespawnRequest> objectDespawnMessage = new("DespawnObject", Imperium.Networking);

    private readonly ImpNetMessage<ExtractionCompleteRequest> extractionCompleteRequest = new(
        "ExtractionComplete", Imperium.Networking
    );

    private readonly ImpNetEvent objectsChangedEvent = new("ObjectsChanged", Imperium.Networking);

    protected override void Init()
    {
        FetchGlobalSpawnLists();
        FetchPlayers();

        RefreshLevelObjects();

        Imperium.IsLevelLoaded.OnTrigger += RefreshLevelObjects;
        Imperium.IsLevelLoaded.OnTrigger += FetchPlayers;

        objectsChangedEvent.OnClientRecive += RefreshLevelObjects;

        if (SemiFunc.IsMasterClientOrSingleplayer())
        {
            entitySpawnMessage.OnServerReceive += OnSpawnEntity;
            itemSpawnMessage.OnServerReceive += OnSpawnItem;
            valuableSpawnMessage.OnServerReceive += OnSpawnValuable;

            objectDespawnMessage.OnServerReceive += OnDespawnObject;
            extractionCompleteRequest.OnServerReceive += OnExtractionComplete;

            enemyTeleportationRequest.OnServerReceive += OnEnemyTeleportRequestServer;
            valuableTeleportationRequest.OnServerReceive += OnValuableTeleportRequestServer;
            itemTeleportationRequest.OnServerReceive += OnItemTeleportRequestServer;
        }
    }

    protected override void OnLevelLoad() => RefreshLevelObjects();

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
    internal void TeleportEnemy(EnemyTeleportRequest request) => enemyTeleportationRequest.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void TeleportValuable(ValuableTeleportRequest request) =>
        valuableTeleportationRequest.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void TeleportItem(ItemTeleportRequest request) => itemTeleportationRequest.DispatchToServer(request);

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

            /*
             * Since enemies with multiple spawn prefabs have a director script as their first prefab, we need to take the
             * second prefab to get the enemy parent here.
             */
            var parent =
                enemyType.spawnObjects.Count > 1
                    ? enemyType.spawnObjects[1].GetComponent<EnemyParent>()
                    : enemyType.spawnObjects[0].GetComponent<EnemyParent>();

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
        var allModules = Resources.FindObjectsOfTypeAll<Module>().OrderBy(module => module.name).ToList();

        var allValuables = Valuables.GetValuables().Select(obj => obj.GetComponent<ValuableObject>()).ToHashSet();

        LoadedItems.Set(allItems);
        LoadedLevels.Set(allLevels);
        LoadedModules.Set(allModules);
        LoadedEntities.Set(allEntities);
        LoadedValuables.Set(allValuables);
    }

    private void RefreshLevelObjects()
    {
        HashSet<EnemyParent> currentLevelEntities = [];
        HashSet<ItemAttributes> currentLevelItems = [];
        HashSet<ValuableObject> currentLevelValuables = [];
        HashSet<ExtractionPoint> currentLevelExtractionPoints = [];

        foreach (var enemy in FindObjectsByType<EnemyParent>(FindObjectsSortMode.None))
        {
            currentLevelEntities.Add(enemy);

            var uniqueId = SemiFunc.IsMultiplayer()
                ? enemy.photonView.ViewID
                : enemy.gameObject.GetInstanceID();
            CurrentLevelObjects[uniqueId] = enemy.gameObject;
        }

        foreach (var item in FindObjectsByType<ItemAttributes>(FindObjectsSortMode.None))
        {
            currentLevelItems.Add(item);

            var uniqueId = SemiFunc.IsMultiplayer()
                ? item.photonView.ViewID
                : item.gameObject.GetInstanceID();
            CurrentLevelObjects[uniqueId] = item.gameObject;
        }

        foreach (var valuable in FindObjectsByType<ValuableObject>(FindObjectsSortMode.None))
        {
            currentLevelValuables.Add(valuable);

            var uniqueId = SemiFunc.IsMultiplayer()
                ? valuable.photonView.ViewID
                : valuable.gameObject.GetInstanceID();
            CurrentLevelObjects[uniqueId] = valuable.gameObject;
        }

        foreach (var extractionPoint in FindObjectsByType<ExtractionPoint>(FindObjectsSortMode.None))
        {
            currentLevelExtractionPoints.Add(extractionPoint);

            var uniqueId = SemiFunc.IsMultiplayer()
                ? extractionPoint.photonView.ViewID
                : extractionPoint.gameObject.GetInstanceID();
            CurrentLevelObjects[uniqueId] = extractionPoint.gameObject;
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
            if (!obj || !obj.TryGetComponent(out EnemyParent enemyParent)) continue;

            enemyParent.SetupDone = true;

            var enemy = obj.GetComponentInChildren<Enemy>();
            if (enemy) enemy.EnemyTeleported(request.SpawnPosition);
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
            if (!obj)
            {
                Imperium.IO.LogError($"[SPAWN] Failed to spawn item '{request.Name}'.");
                return;
            }
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
            if (!obj)
            {
                Imperium.IO.LogError($"[SPAWN] Failed to spawn valuable '{request.Name}'.");
                return;
            }
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
    private void OnItemTeleportRequestServer(ItemTeleportRequest request, ulong clientId)
    {
        if (!CurrentLevelObjects.TryGetValue(request.ObjectId, out var obj))
        {
            Imperium.IO.LogInfo($"[OBJ] Failed to find item to teleport with ID '{request.ObjectId}'");
            return;
        }

        if (!obj.TryGetComponent<ItemAttributes>(out var item))
        {
            Imperium.IO.LogInfo($"[OBJ] Failed to find ItemAttributes component on object with ID '{request.ObjectId}'");
            return;
        }

        item.gameObject.transform.position = request.Destination;
    }

    [ImpAttributes.HostOnly]
    private void OnValuableTeleportRequestServer(ValuableTeleportRequest request, ulong clientId)
    {
        if (!CurrentLevelObjects.TryGetValue(request.ObjectId, out var obj))
        {
            Imperium.IO.LogInfo($"[OBJ] Failed to find valuable to teleport with ID '{request.ObjectId}'");
            return;
        }

        if (!obj.TryGetComponent<ValuableObject>(out var valuableObject))
        {
            Imperium.IO.LogInfo($"[OBJ] Failed to find ValuableObject component on object with ID '{request.ObjectId}'");
            return;
        }

        valuableObject.gameObject.transform.position = request.Destination;
    }

    [ImpAttributes.HostOnly]
    private void OnEnemyTeleportRequestServer(EnemyTeleportRequest request, ulong senderId)
    {
        if (!CurrentLevelObjects.TryGetValue(request.ObjectId, out var obj))
        {
            Imperium.IO.LogInfo($"[OBJ] Failed to find enemy to teleport with ID '{request.ObjectId}'");
            return;
        }

        if (!obj.TryGetComponent<EnemyParent>(out var enemy))
        {
            Imperium.IO.LogInfo($"[OBJ] Failed to find EnemyParent component on object with ID '{request.ObjectId}'");
            return;
        }

        enemy.Enemy.EnemyTeleported(request.Destination);
    }

    [ImpAttributes.HostOnly]
    private void OnExtractionComplete(ExtractionCompleteRequest request, ulong senderId)
    {
        if (!CurrentLevelObjects.TryGetValue(request.ObjectId, out var obj))
        {
            Imperium.IO.LogInfo($"[OBJ] Failed to find extraction point to complete with ID '{request.ObjectId}'");
            return;
        }

        if (!obj.TryGetComponent<ExtractionPoint>(out var extractionPoint))
        {
            Imperium.IO.LogInfo($"[OBJ] Failed to find ExtractionPoint component on object with ID '{request.ObjectId}'");
            return;
        }

        extractionPoint.StateSet(ExtractionPoint.State.Complete);
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnObject(ObjectDespawnRequest request, ulong senderId)
    {
        if (!CurrentLevelObjects.TryGetValue(request.ObjectId, out var obj))
        {
            Imperium.IO.LogError($"[SPAWN] [R] Failed to despawn object with view ID {request.ObjectId}");
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