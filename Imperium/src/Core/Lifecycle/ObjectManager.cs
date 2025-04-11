#region

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Imperium.API.Types.Networking;
using Imperium.Core.Scripts;
using Imperium.Netcode;
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

    private readonly Dictionary<string, string> displayNameMap = [];
    private readonly Dictionary<string, string> overrideDisplayNameMap = [];

    private readonly ImpNetMessage<EntitySpawnRequest> entitySpawnMessage = new("SpawnEntity", Imperium.Networking);
    private readonly ImpNetMessage<ItemSpawnRequest> itemSpawnMessage = new("SpawnItem", Imperium.Networking);
    private readonly ImpNetMessage<ValuableSpawnRequest> valuableSpawnMessage = new("SpawnValuable", Imperium.Networking);

    private readonly ImpNetMessage<MapHazardSpawnRequest> mapHazardSpawnMessage = new(
        "MapHazardSpawn", Imperium.Networking
    );

    private readonly ImpNetMessage<StaticPrefabSpawnRequest> staticPrefabSpawnMessage = new(
        "StaticPrefabSpawn", Imperium.Networking
    );

    private readonly ImpNetMessage<StaticPrefabSpawnRequest> localStaticPrefabSpawnMessage = new(
        "LocalStaticPrefabSpawn", Imperium.Networking
    );

    private readonly ImpNetMessage<StaticPrefabSpawnRequest> outsideObjectPrefabSpawnMessage = new(
        "OutsideObjectSpawn", Imperium.Networking
    );

    private readonly ImpNetMessage<CompanyCruiserSpawnRequest> companyCruiserSpawnMessage = new(
        "CompanyCruiserSpawn", Imperium.Networking
    );

    private readonly ImpNetMessage<ObjectTeleportRequest> objectTeleportationRequest = new(
        "ObjectTeleportation", Imperium.Networking
    );

    private readonly ImpNetMessage<LocalObjectTeleportRequest> localObjectTeleportationRequest = new(
        "LocalObjectTeleportation", Imperium.Networking
    );

    private readonly ImpNetMessage<int> burstSteamValve = new("BurstSteamValve", Imperium.Networking);
    private readonly ImpNetMessage<EntityDespawnRequest> entityDespawnMessage = new("DespawnEntity", Imperium.Networking);
    private readonly ImpNetMessage<int> itemDespawnMessage = new("DespawnItem", Imperium.Networking);
    private readonly ImpNetMessage<int> obstacleDespawnMessage = new("DespawnObstacle", Imperium.Networking);

    private readonly ImpNetMessage<LocalObjectDespawnRequest> localObjectDespawnMessage = new(
        "DespawnLocalObject", Imperium.Networking
    );

    private readonly ImpNetEvent objectsChangedEvent = new("ObjectsChanged", Imperium.Networking);

    // List of prefab names of outside objects. Used to identify outside objects.
    private readonly HashSet<string> OutsideObjectPrefabNameMap =
    [
        "GiantPumpkin(Clone)",
        "LargeRock1(Clone)",
        "LargeRock2(Clone)",
        "LargeRock3(Clone)",
        "LargeRock4(Clone)",
        "GreyRockGrouping2(Clone)",
        "GreyRockGrouping4(Clone)",
        "tree(Clone)",
        "treeLeaflessBrown.001 Variant(Clone)",
        "treeLeafless.002_LOD0(Clone)",
        "treeLeafless.003_LOD0(Clone)"
    ];

    /*
     * Collections for the entity name system.
     */
    private readonly List<string> AvailableEntityNames = ImpAssets.EntityNames.Select(entityName => entityName).ToList();
    private readonly Dictionary<int, string> EntityNameMap = [];

    private readonly HashSet<int> SpawnedEntityIds = [];

    /*
     * Assets loaded from the game's resources after loading objects
     */
    // internal AudioClip BeaconDrop;

    protected override void Init()
    {
        FetchGlobalSpawnLists();
        FetchPlayers();

        RefreshLevelObjects();

        LogObjects();

        objectsChangedEvent.OnClientRecive += RefreshLevelObjects;
        burstSteamValve.OnClientRecive += OnSteamValveBurst;
        objectTeleportationRequest.OnClientRecive += OnObjectTeleportationRequestClient;

        localObjectDespawnMessage.OnClientRecive += OnDespawnLocalObject;
        localStaticPrefabSpawnMessage.OnClientRecive += OnSpawnLocalStaticPrefabClient;
        outsideObjectPrefabSpawnMessage.OnClientRecive += OnSpawnOutsideObjectClient;
        localObjectTeleportationRequest.OnClientRecive += OnLocalObjectTeleportationRequestClient;

        if (PhotonNetwork.IsMasterClient)
        {
            entitySpawnMessage.OnServerReceive += OnSpawnEntity;
            itemSpawnMessage.OnServerReceive += OnSpawnItem;
            valuableSpawnMessage.OnServerReceive += OnSpawnValuable;
            mapHazardSpawnMessage.OnServerReceive += OnSpawnMapHazard;
            companyCruiserSpawnMessage.OnServerReceive += OnSpawnCompanyCruiser;
            staticPrefabSpawnMessage.OnServerReceive += OnSpawnStaticPrefabServer;

            // entityDespawnMessage.OnServerReceive += OnDespawnEntity;
            // itemDespawnMessage.OnServerReceive += OnDespawnItem;
            // obstacleDespawnMessage.OnServerReceive += OnDespawnObstacle;

            objectTeleportationRequest.OnServerReceive += OnObjectTeleportationRequestServer;
        }
    }

    protected override void OnSceneLoad()
    {
        RefreshLevelObjects();

        LogObjects();

        // Reload objects that are hidden on the moon but visible in space
        Imperium.Settings.Rendering.SpaceSun.Refresh();
        Imperium.Settings.Rendering.StarsOverlay.Refresh();
    }

    protected override void OnPlayersUpdate(int playersConnected) => FetchPlayers();

    [ImpAttributes.RemoteMethod]
    internal void SpawnEntity(EntitySpawnRequest request) => entitySpawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void SpawnItem(ItemSpawnRequest request) => itemSpawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void SpawnValuable(ValuableSpawnRequest request) => valuableSpawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void SpawnMapHazard(MapHazardSpawnRequest request) => mapHazardSpawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void SpawnStaticPrefab(StaticPrefabSpawnRequest request)
    {
        if (!LoadedStaticPrefabs.Value.ContainsKey(request.Name))
        {
            Imperium.IO.LogError($"[SPAWN] Unable to find requested static prefab '{request.Name}'.");
            return;
        }

        staticPrefabSpawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void SpawnLocalStaticPrefab(StaticPrefabSpawnRequest request)
    {
        if (!LoadedLocalStaticPrefabs.Value.ContainsKey(request.Name))
        {
            Imperium.IO.LogError($"[SPAWN] Unable to find requested local static prefab '{request.Name}'.");
            return;
        }

        localStaticPrefabSpawnMessage.DispatchToClients(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void SpawnOutsideObject(StaticPrefabSpawnRequest request)
    {
        // if (!LoadedOutsideObjects.Value.ContainsKey(request.Name))
        // {
        //     Imperium.IO.LogError($"[SPAWN] Unable to find requested outside object '{request.Name}'.");
        //     return;
        // }

        outsideObjectPrefabSpawnMessage.DispatchToClients(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void SpawnCompanyCruiser(CompanyCruiserSpawnRequest request)
    {
        companyCruiserSpawnMessage.DispatchToServer(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void DespawnItem(int itemNetId) => itemDespawnMessage.DispatchToServer(itemNetId);

    [ImpAttributes.RemoteMethod]
    internal void DespawnEntity(EntityDespawnRequest request) => entityDespawnMessage.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void DespawnObstacle(int obstacleNetId) => obstacleDespawnMessage.DispatchToServer(obstacleNetId);

    [ImpAttributes.RemoteMethod]
    internal void DespawnLocalObject(LocalObjectDespawnRequest request)
    {
        localObjectDespawnMessage.DispatchToClients(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void TeleportObject(ObjectTeleportRequest request) => objectTeleportationRequest.DispatchToServer(request);

    [ImpAttributes.RemoteMethod]
    internal void TeleportLocalObject(LocalObjectTeleportRequest request)
    {
        localObjectTeleportationRequest.DispatchToClients(request);
    }

    [ImpAttributes.RemoteMethod]
    internal void InvokeObjectsChanged() => objectsChangedEvent.DispatchToClients();

    [ImpAttributes.RemoteMethod]
    // internal void BurstSteamValve(ulong valveNetId) => burstSteamValve.DispatchToClients(valveNetId);
    internal string GetDisplayName(string inGameName) => displayNameMap.GetValueOrDefault(inGameName, inGameName);

    internal string GetOverrideDisplayName(string inGameName) => overrideDisplayNameMap.GetValueOrDefault(inGameName);

    [ImpAttributes.LocalMethod]
    internal void EmptyVent(ulong netId)
    {
        // if (!CurrentLevelObjects.TryGetValue(netId, out var obj) ||
        //     !obj.TryGetComponent<EnemyVent>(out var enemyVent))
        // {
        //     Imperium.IO.LogError($"Failed to empty vent with net ID {netId}");
        //     return;
        // }

        // enemyVent.occupied = false;
    }

    internal GameObject FindObject(string objName)
    {
        if (ObjectCache.TryGetValue(objName, out var v)) return v;
        var obj = Resources.FindObjectsOfTypeAll<GameObject>().FirstOrDefault(
            obj => obj.name == objName && obj.scene != SceneManager.GetSceneByName("HideAndDontSave"));
        if (!obj) return null;
        ObjectCache[objName] = obj;
        return obj;
    }

    internal void ToggleObject(string objName, bool isOn)
    {
        var obj = FindObject(objName);
        if (obj) obj.SetActive(isOn);
    }

    /// <summary>
    ///     Fetches all game objects from resources to be used later for spawning
    ///     - Entities (Indoor, Outdoor, Daytime)
    ///     - Scrap and Items
    ///     - Map Hazards
    ///     - Other Static Prefabs (e.g. clipboard, player body)
    /// </summary>
    private void FetchGlobalSpawnLists()
    {
        var allEntities = new HashSet<ExtendedEnemySetup>();

        // EnemyType redPillType = null;
        // var shiggyExists = false;

        foreach (var enemyType in EnemyDirector.instance.GetEnemies())
        {
            if (enemyType.spawnObjects.Count < 1) continue;
            var parent = enemyType.spawnObjects[0].GetComponent<EnemyParent>();
            if (!parent) continue;

            Imperium.IO.LogInfo($"PARENT IS NULL: {parent}");

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
            // allEntities.Add(enemyType);

            // switch (enemyType.spawnObjects[0].GetComponent<EnemyParent>().enemyName)
            // {
            //     case "Red pill":
            //         redPillType = enemyType;
            //         break;
            //     case "Shiggy":
            //         shiggyExists = true;
            //         break;
            // }
        }

        // Instantiate shiggy type if not already exists and if redpill has been found
        // if (redPillType && !shiggyExists) allEntities.Add(CreateShiggyType(redPillType));

        var allItems = Resources.FindObjectsOfTypeAll<Item>()
            .Where(item => item.prefab && !ImpConstants.ItemBlacklist.Contains(item.itemName))
            .ToHashSet();

        var allValuables = Valuables.GetValuables().Select(obj => obj.GetComponent<ValuableObject>()).ToHashSet();
        // BeaconDrop = allItems.First(item => item.itemName == "Radar-booster").dropSFX;

        // var allScrap = allItems.Where(scrap => scrap.isScrap).ToHashSet();
        //
        // var allMapHazards = new Dictionary<string, GameObject>();
        // var allStaticPrefabs = new Dictionary<string, GameObject>();
        // var allLocalStaticPrefabs = new Dictionary<string, GameObject>();
        // var allOutsideObjects = Resources.FindObjectsOfTypeAll<SpawnableOutsideObject>()
        //     .GroupBy(obj => obj.prefabToSpawn.name)
        //     .Select(obj => obj.First())
        //     .ToDictionary(obj => obj.prefabToSpawn.name);

        // foreach (var obj in Resources.FindObjectsOfTypeAll<GameObject>())
        // {
        //     switch (obj.name)
        //     {
        //         case "SpikeRoofTrapHazard":
        //             allMapHazards["Spike Trap"] = obj;
        //             break;
        //         case "TurretContainer":
        //             allMapHazards["Turret"] = obj;
        //             break;
        //         case "SteamValve":
        //             allMapHazards["SteamValve"] = obj;
        //             break;
        //         // Find all landmine containers (Not the actual mine objects which happen to have the same name)
        //         case "Landmine" when obj.transform.Find("Landmine"):
        //             allMapHazards["Landmine"] = obj;
        //             break;
        //         case "CompanyCruiser":
        //             allStaticPrefabs["CompanyCruiser"] = obj;
        //             break;
        //         case "CompanyCruiserManual":
        //             allStaticPrefabs["CompanyCruiserManual"] = obj;
        //             break;
        //         case "RagdollGrabbableObject":
        //             allStaticPrefabs["Body"] = obj;
        //             break;
        //         case "ClipboardManual":
        //             allStaticPrefabs["Clipboard"] = obj;
        //             break;
        //         case "StickyNoteItem":
        //             allStaticPrefabs["StickyNote"] = obj;
        //             break;
        //     }
        // }

        LoadedItems.Set(allItems);
        LoadedEntities.Set(allEntities);
        LoadedValuables.Set(allValuables);
        // LoadedMapHazards.Set(allMapHazards);
        // LoadedStaticPrefabs.Set(allStaticPrefabs);
        // LoadedOutsideObjects.Set(allOutsideObjects);
        // LoadedLocalStaticPrefabs.Set(allLocalStaticPrefabs);

        GenerateDisplayNameMaps();
    }


    // private static EnemyType CreateShiggyType(EnemyType type)
    // {
    //     var shiggyType = Instantiate(type);
    //     shiggyType.enemyName = "Shiggy";
    //
    //     return shiggyType;
    // }

    internal string GetEntityName(ExtendedEnemySetup entity)
    {
        // if (!EntityNameMap.TryGetValue(instanceId, out var entityName))
        // {
        //     if (AvailableEntityNames.Count == 0)
        //     {
        //         Imperium.IO.LogInfo("[OBJ] Somehow Imperium is out of entity names. Falling back to instance ID.");
        //         return instanceId.ToString();
        //     }
        //
        //     var newNameIndex = Random.Range(0, AvailableEntityNames.Count);
        //
        //     entityName = AvailableEntityNames[newNameIndex];
        //     EntityNameMap[instanceId] = entityName;
        //
        //     AvailableEntityNames.RemoveAt(newNameIndex);
        // }

        return entity.EnemyName;
    }

    // internal void RefreshLevelEntities()
    // {
    //     HashSet<EnemyAI> currentLevelEntities = [];
    //     foreach (var obj in FindObjectsOfType<EnemyAI>())
    //     {
    //         // Ignore objects that are hidden
    //         if (obj.gameObject.scene == SceneManager.GetSceneByName("HideAndDontSave")) continue;
    //
    //         currentLevelEntities.Add(obj);
    //         CurrentLevelObjects[obj.GetComponent<NetworkObject>().NetworkObjectId] = obj.gameObject;
    //     }
    //
    //     CurrentLevelEntities.Set(currentLevelEntities);
    //     CurrentLevelObjectsChanged?.Invoke();
    // }

    private readonly LayerMask terrainMask = LayerMask.NameToLayer("Terrain");

    internal void RefreshLevelObjects()
    {
        HashSet<EnemyParent> currentLevelEntities = [];
        HashSet<ItemAttributes> currentLevelItems = [];
        HashSet<ValuableObject> currentLevelValuables = [];

        foreach (var obj in FindObjectsOfType<GameObject>())
        {
            // if (obj.layer == terrainMask
            //     && OutsideObjectPrefabNameMap.Contains(obj.name)
            //     && currentLevelOutsideObjects.Add(obj)
            //    )
            // {
            //     continue;
            // }

            foreach (var component in obj.GetComponents<Component>())
            {
                switch (component)
                {
                    case EnemyParent entity:
                        currentLevelEntities.Add(entity);
                        CurrentLevelObjects[entity.photonView.ViewID] = obj.gameObject;
                        break;
                    case ItemAttributes item:
                        currentLevelItems.Add(item);
                        CurrentLevelObjects[item.photonView.ViewID] = obj.gameObject;
                        break;
                    case ValuableObject valuable:
                        currentLevelValuables.Add(valuable);
                        CurrentLevelObjects[valuable.photonView.ViewID] = obj.gameObject;
                        break;
                }
            }
        }

        CurrentLevelItems.Set(currentLevelItems);
        CurrentLevelEntities.Set(currentLevelEntities);
        CurrentLevelValuables.Set(currentLevelValuables);
        // CurrentLevelDoors.Set(currentLevelDoors);
        // CurrentLevelSecurityDoors.Set(currentLevelSecurityDoors);
        // CurrentLevelTurrets.Set(currentLevelTurrets);
        // CurrentLevelLandmines.Set(currentLevelLandmines);
        // CurrentLevelSpikeTraps.Set(currentLevelSpikeTraps);
        // CurrentLevelBreakerBoxes.Set(currentLevelBreakerBoxes);
        // CurrentLevelVents.Set(currentLevelVents);
        // CurrentLevelSteamValves.Set(currentLevelSteamValves);
        // CurrentLevelSpiderWebs.Set(currentLevelSpiderWebs);
        // CurrentScrapSpawnPoints.Set(currentScrapSpawnPoints);
        // CurrentLevelCruisers.Set(currentLevelCompanyCruisers);

        // stopwatch.Stop();
        // Imperium.IO.LogInfo($"REFRESH : {stopwatch.ElapsedMilliseconds}");

        CurrentLevelObjectsChanged?.Invoke();

        // stopwatch2.Stop();
        // Imperium.IO.LogInfo($"TOTAL REFRESH : {stopwatch2.ElapsedMilliseconds}");
    }

    private void GenerateDisplayNameMaps()
    {
        // foreach (var entity in LoadedEntities.Value)
        // {
        //     if (!entity.enemyPrefab) continue;
        //     var displayName = entity.enemyPrefab.GetComponentInChildren<ScanNodeProperties>()?.headerText;
        //     if (!string.IsNullOrEmpty(displayName)) displayNameMap[entity.enemyName] = displayName;
        // }
        //
        // foreach (var item in LoadedItems.Value)
        // {
        //     if (!item.spawnPrefab) continue;
        //     var displayName = item.spawnPrefab.GetComponentInChildren<ScanNodeProperties>()?.headerText;
        //     if (!string.IsNullOrEmpty(displayName)) displayNameMap[item.itemName] = displayName;
        // }

        overrideDisplayNameMap["StickyNote"] = "Sticky Note";
        overrideDisplayNameMap["Clipboard"] = "Clipboard";
        overrideDisplayNameMap["CompanyCruiserManual"] = "Company Cruiser Manual";
        overrideDisplayNameMap["Body"] = "Player Body";
        overrideDisplayNameMap["GiantPumpkin"] = "Giant Pumpkin";
        overrideDisplayNameMap["LargeRock1"] = "Large Rock 1";
        overrideDisplayNameMap["LargeRock2"] = "Large Rock 2";
        overrideDisplayNameMap["LargeRock3"] = "Large Rock 3";
        overrideDisplayNameMap["LargeRock4"] = "Large Rock 4";
        overrideDisplayNameMap["GreyRockGrouping2"] = "Grey Rock Grouping 2";
        overrideDisplayNameMap["GreyRockGrouping4"] = "Grey Rock Grouping 4";
        overrideDisplayNameMap["tree"] = "Tree";
        overrideDisplayNameMap["treeLeafless"] = "Tree Leafless";
        overrideDisplayNameMap["treeLeaflessBrown.001 Variant"] = "Tree Leafless Brown";
        overrideDisplayNameMap["treeLeafless.002_LOD0"] = "Tree Leafless 2 (Snowy)";
        overrideDisplayNameMap["treeLeafless.003_LOD0"] = "Tree Leafless 3 (Snowy)";

        // Copied names for instantiated objects
        overrideDisplayNameMap["GiantPumpkin(Clone)"] = "Giant Pumpkin";
        overrideDisplayNameMap["LargeRock1(Clone)"] = "Large Rock 1";
        overrideDisplayNameMap["LargeRock2(Clone)"] = "Large Rock 2";
        overrideDisplayNameMap["LargeRock3(Clone)"] = "Large Rock 3";
        overrideDisplayNameMap["LargeRock4(Clone)"] = "Large Rock 4";
        overrideDisplayNameMap["GreyRockGrouping2(Clone)"] = "Grey Rock Grouping 2";
        overrideDisplayNameMap["GreyRockGrouping4(Clone)"] = "Grey Rock Grouping 4";
        overrideDisplayNameMap["tree(Clone)"] = "Tree";
        overrideDisplayNameMap["treeLeaflessBrown.001 Variant(Clone)"] = "Tree Leafless 1";
        overrideDisplayNameMap["treeLeafless.002_LOD0(Clone)"] = "Tree Leafless 2 (Snowy)";
        overrideDisplayNameMap["treeLeafless.003_LOD0(Clone)"] = "Tree Leafless 3 (Snowy)";
    }

    private void FetchPlayers()
    {
        CurrentPlayers.Set(GameDirector.instance.PlayerList.ToHashSet());
    }

    private void LogObjects()
    {
        // Imperium.IO.LogBlock([
        //     "Imperium scanned the current level for obstacles.",
        //     $"   > {CurrentLevelDoors.Value.Count}x Doors",
        //     $"   > {CurrentLevelSecurityDoors.Value.Count}x Security doors",
        //     $"   > {CurrentLevelTurrets.Value.Count}x Turrets",
        //     $"   > {CurrentLevelLandmines.Value.Count}x Landmines",
        //     $"   > {CurrentLevelBreakerBoxes.Value.Count}x Breaker boxes",
        //     $"   > {CurrentLevelSpiderWebs.Value.Count}x Spider webs"
        // ]);
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
                Imperium.IO.LogInfo($"enemy: {enemy}");
                Imperium.IO.LogInfo($"vview: {enemy.photonView}");
                CurrentLevelObjects[enemy.photonView.ViewID] = obj;
                SpawnedEntityIds.Add(enemy.photonView.ViewID);
                enemy.EnemyTeleported(request.SpawnPosition);
            }
            else
            {
                Imperium.IO.LogInfo("Spawned entity that is null");
            }
        }

        if (request.SendNotification)
        {
            var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
            var verbString = request.Amount == 1 ? "has" : "have";

            Imperium.Networking.SendLog(new NetworkNotification
            {
                Message = $"{mountString} loyal {GetDisplayName(request.Name)} {verbString} been spawned!",
                Type = NotificationType.Spawning
            });
        }

        objectsChangedEvent.DispatchToClients();
    }

    // private static GameObject InstantiateShiggy(EnemyType enemyType, Vector3 spawnPosition)
    // {
    //     // var shiggyPrefab = Instantiate(enemyType.enemyPrefab, spawnPosition, Quaternion.identity);
    //     // shiggyPrefab.name = "ShiggyEntity";
    //     // Destroy(shiggyPrefab.GetComponent<TestEnemy>());
    //     // Destroy(shiggyPrefab.GetComponent<HDAdditionalLightData>());
    //     // Destroy(shiggyPrefab.GetComponent<Light>());
    //     // Destroy(shiggyPrefab.GetComponent<AudioSource>());
    //     // foreach (var componentsInChild in shiggyPrefab.GetComponentsInChildren<BoxCollider>())
    //     // {
    //     //     Destroy(componentsInChild);
    //     // }
    //     //
    //     // var shiggyAI = shiggyPrefab.AddComponent<ShiggyAI>();
    //     // shiggyAI.enemyType = enemyType;
    //     //
    //     // return shiggyPrefab;
    // }

    [ImpAttributes.LocalMethod]
    private void DespawnLocalObject(LocalObjectType type, Vector3 position, GameObject obj)
    {
        if (!obj)
        {
            Imperium.IO.LogError(
                $"[SPAWN] [R] Failed to despawn local object of type '{type}' at {Formatting.FormatVector(position)}."
            );
            return;
        }

        Destroy(obj);
        RefreshLevelObjects();
    }

    [ImpAttributes.LocalMethod]
    private static void TeleportLocalObject(LocalObjectType type, Vector3 position, GameObject obj, Vector3 destination)
    {
        if (!obj)
        {
            Imperium.IO.LogError(
                $"[SPAWN] [R] Failed to local teleport object of type '{type}' at {Formatting.FormatVector(position)}."
            );
            return;
        }

        obj.transform.position = destination;
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
    private void OnSpawnMapHazard(MapHazardSpawnRequest request, ulong clientId)
    {
        // for (var i = 0; i < request.Amount; i++)
        // {
        //     switch (request.Name)
        //     {
        //         case "Turret":
        //             SpawnTurret(request.SpawnPosition);
        //             break;
        //         case "Spike Trap":
        //             SpawnSpikeTrap(request.SpawnPosition);
        //             break;
        //         case "Landmine":
        //             SpawnLandmine(request.SpawnPosition);
        //             break;
        //         case "SteamValve":
        //             SpawnSteamValve(request.SpawnPosition);
        //             break;
        //         default:
        //             Imperium.IO.LogError($"[SPAWN] [R] Failed to spawn map hazard {request.Name}");
        //             return;
        //     }
        // }
        //
        // var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
        // var verbString = request.Amount == 1 ? "has" : "have";
        //
        // if (request.SendNotification)
        // {
        //     Imperium.Networking.SendLog(new NetworkNotification
        //     {
        //         Message = $"{mountString} {request.Name} {verbString} been spawned!",
        //         Type = NotificationType.Spawning
        //     });
        // }
        //
        // objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.LocalMethod]
    private void OnSpawnOutsideObjectClient(StaticPrefabSpawnRequest request)
    {
        // if (!LoadedOutsideObjects.Value.TryGetValue(request.Name, out var outsideObject))
        // {
        //     Imperium.IO.LogError($"[SPAWN] [R] Unable to find outside object '{request.Name}'.");
        //     return;
        // }
        //
        // for (var i = 0; i < request.Amount; i++)
        // {
        //     Instantiate(
        //         outsideObject.prefabToSpawn, request.SpawnPosition, Quaternion.Euler(outsideObject.rotationOffset)
        //     );
        // }
        //
        // if (request.SendNotification)
        // {
        //     var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
        //     var verbString = request.Amount == 1 ? "has" : "have";
        //
        //     var objectName = overrideDisplayNameMap.GetValueOrDefault(request.Name)
        //                      ?? displayNameMap.GetValueOrDefault(request.Name)
        //                      ?? request.Name;
        //
        //     Imperium.IO.Send(
        //         $"{mountString} {objectName} {verbString} been spawned!",
        //         type: NotificationType.Spawning
        //     );
        // }
        //
        // RefreshLevelObjects();
    }

    [ImpAttributes.LocalMethod]
    private void OnSpawnLocalStaticPrefabClient(StaticPrefabSpawnRequest request)
    {
        // if (!LoadedLocalStaticPrefabs.Value.TryGetValue(request.Name, out var staticPrefab))
        // {
        //     Imperium.IO.LogError($"[SPAWN] [R] Unable to find local static prefab '{request.Name}'.");
        //     return;
        // }
        //
        // var rotationOffset = Quaternion.identity;
        //
        // if (staticPrefab.TryGetComponent<SpawnableOutsideObject>(out var outsideObject))
        // {
        //     rotationOffset = Quaternion.Euler(outsideObject.rotationOffset);
        // }
        //
        // for (var i = 0; i < request.Amount; i++)
        // {
        //     Instantiate(staticPrefab, request.SpawnPosition, rotationOffset);
        // }
        //
        // if (request.SendNotification)
        // {
        //     var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
        //     var verbString = request.Amount == 1 ? "has" : "have";
        //
        //     var objectName = overrideDisplayNameMap.GetValueOrDefault(request.Name)
        //                      ?? displayNameMap.GetValueOrDefault(request.Name)
        //                      ?? request.Name;
        //
        //     Imperium.IO.Send(
        //         $"{mountString} {objectName} {verbString} been spawned!",
        //         type: NotificationType.Spawning
        //     );
        // }
        //
        // RefreshLevelObjects();
    }

    [ImpAttributes.HostOnly]
    private void OnSpawnCompanyCruiser(CompanyCruiserSpawnRequest request, ulong clientId)
    {
        // Raycast to find the ground to spawn the entity on
        // var hasGround = Physics.Raycast(
        //     new Ray(request.SpawnPosition + Vector3.up * 2f, Vector3.down),
        //     out var groundInfo, 100, ImpConstants.IndicatorMask
        // );
        // var actualSpawnPosition = hasGround
        //     ? groundInfo.point
        //     : clientId.GetPlayerController()!.transform.position;
        //
        // var cruiserObj = Instantiate(
        //     LoadedStaticPrefabs.Value["CompanyCruiser"],
        //     actualSpawnPosition + Vector3.up * 2.5f,
        //     Quaternion.identity,
        //     RoundManager.Instance.VehiclesContainer
        // );
        //
        // var vehicleNetObject = cruiserObj.gameObject.GetComponentInChildren<NetworkObject>();
        // vehicleNetObject.Spawn();
        // CurrentLevelObjects[vehicleNetObject.NetworkObjectId] = cruiserObj;
        //
        // var cruiserManualObj = Instantiate(
        //     LoadedStaticPrefabs.Value["CompanyCruiserManual"],
        //     actualSpawnPosition + Vector3.up * 2.5f,
        //     Quaternion.identity,
        //     RoundManager.Instance.VehiclesContainer
        // );
        // var manualNetObject = cruiserManualObj.gameObject.GetComponentInChildren<NetworkObject>();
        // manualNetObject.Spawn();
        // CurrentLevelObjects[manualNetObject.NetworkObjectId] = cruiserObj;
        //
        // if (request.SendNotification)
        // {
        //     Imperium.Networking.SendLog(new NetworkNotification
        //     {
        //         Message = "A trusty Company Cruiser has been spawned!",
        //         Type = NotificationType.Spawning
        //     });
        // }
        //
        // objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void OnSpawnStaticPrefabServer(StaticPrefabSpawnRequest request, ulong client)
    {
        // if (!LoadedStaticPrefabs.Value.TryGetValue(request.Name, out var staticPrefab))
        // {
        //     Imperium.IO.LogError($"[SPAWN] [R] Unable to find static prefab '{request.Name}' requested by {client}.");
        //     return;
        // }
        //
        // for (var i = 0; i < request.Amount; i++)
        // {
        //     var staticObj = Instantiate(staticPrefab, request.SpawnPosition, Quaternion.Euler(Vector3.zero));
        //
        //     var netObject = staticObj.gameObject.GetComponent<NetworkObject>();
        //     netObject.Spawn(destroyWithScene: true);
        //
        //     CurrentLevelObjects[netObject.NetworkObjectId] = staticObj;
        // }
        //
        // if (request.SendNotification)
        // {
        //     var mountString = request.Amount == 1 ? "A" : $"{request.Amount.ToString()}x";
        //     var verbString = request.Amount == 1 ? "has" : "have";
        //
        //     Imperium.Networking.SendLog(new NetworkNotification
        //     {
        //         Message = $"{mountString} {request.Name} {verbString} been spawned!",
        //         Type = NotificationType.Spawning
        //     });
        // }
        //
        // objectsChangedEvent.DispatchToClients();
    }

    [ImpAttributes.HostOnly]
    private void SpawnLandmine(Vector3 position)
    {
        // var hazardObj = Instantiate(
        //     LoadedMapHazards.Value["Landmine"], position, Quaternion.Euler(Vector3.zero)
        // );
        // hazardObj.transform.Find("Landmine").rotation = Quaternion.Euler(270, 0, 0);
        // hazardObj.transform.localScale = new Vector3(0.4574f, 0.4574f, 0.4574f);
        //
        // var netObject = hazardObj.gameObject.GetComponentInChildren<NetworkObject>();
        // netObject.Spawn(destroyWithScene: true);
        // CurrentLevelObjects[netObject.NetworkObjectId] = hazardObj;
    }

    [ImpAttributes.HostOnly]
    private void SpawnTurret(Vector3 position)
    {
        // var hazardObj = Instantiate(LoadedMapHazards.Value["Turret"], position, Quaternion.Euler(Vector3.zero));
        //
        // var netObject = hazardObj.gameObject.GetComponentInChildren<NetworkObject>();
        // netObject.Spawn(destroyWithScene: true);
        // CurrentLevelObjects[netObject.NetworkObjectId] = hazardObj;
    }

    [ImpAttributes.HostOnly]
    private void SpawnSteamValve(Vector3 position)
    {
        // var hazardObj =
        //     Instantiate(LoadedMapHazards.Value["SteamValve"], position, Quaternion.Euler(Vector3.zero));
        //
        // var netObject = hazardObj.gameObject.GetComponentInChildren<NetworkObject>();
        // netObject.Spawn(destroyWithScene: true);
        // CurrentLevelObjects[netObject.NetworkObjectId] = hazardObj;
    }

    [ImpAttributes.HostOnly]
    private void SpawnSpikeTrap(Vector3 position)
    {
        // var hazardObj = Instantiate(
        //     LoadedMapHazards.Value["Spike Trap"],
        //     position,
        //     Quaternion.Euler(Vector3.zero)
        // );
        //
        // var netObject = hazardObj.gameObject.GetComponentInChildren<NetworkObject>();
        // netObject.Spawn(destroyWithScene: true);
        // CurrentLevelObjects[netObject.NetworkObjectId] = hazardObj;
    }

    [ImpAttributes.HostOnly]
    private void OnObjectTeleportationRequestServer(ObjectTeleportRequest request, ulong clientId)
    {
        objectTeleportationRequest.DispatchToClients(request);
    }

    [ImpAttributes.LocalMethod]
    private void OnObjectTeleportationRequestClient(ObjectTeleportRequest request)
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

    [ImpAttributes.LocalMethod]
    private void OnLocalObjectTeleportationRequestClient(LocalObjectTeleportRequest request)
    {
        // switch (request.Type)
        // {
        //     case LocalObjectType.OutsideObject:
        //         TeleportLocalObject(
        //             request.Type,
        //             request.Position,
        //             CurrentLevelOutsideObjects.Value
        //                 .Where(obj => obj)
        //                 .FirstOrDefault(obj => obj.transform.position == request.Position),
        //             request.Destination
        //         );
        //         break;
        //     default:
        //         Imperium.IO.LogError($"[NET] Local teleportation request has invalid outside object type '{request.Type}'");
        //         break;
        // }
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnItem(int itemNetId, int clientId)
    {
        if (!CurrentLevelObjects.TryGetValue(itemNetId, out var obj))
        {
            Imperium.IO.LogError($"[SPAWN] [R] Failed to despawn item with net ID {itemNetId}");
            return;
        }

        DespawnObject(obj, clientId);
    }

    [ImpAttributes.HostOnly]
    private void OnDespawnEntity(EntityDespawnRequest request, int clientId)
    {
        // if (!CurrentLevelObjects.TryGetValue(request.NetId, out var obj))
        // {
        //     Imperium.IO.LogError($"[SPAWN] [R] Failed to despawn entity with net ID {request.NetId}");
        //     return;
        // }
        //
        // DespawnObject(obj, clientId, request.IsRespawn);
    }

    [ImpAttributes.LocalMethod]
    private void OnDespawnLocalObject(LocalObjectDespawnRequest request)
    {
        // switch (request.Type)
        // {
        //     case LocalObjectType.OutsideObject:
        //         DespawnLocalObject(request.Type, request.Position, CurrentLevelOutsideObjects.Value
        //             .Where(obj => obj)
        //             .FirstOrDefault(obj => obj.transform.position == request.Position)
        //         );
        //         break;
        //     default:
        //         Imperium.IO.LogError($"[NET] Despawn request has invalid outside object type '{request.Type}'");
        //         break;
        // }
    }

    [ImpAttributes.HostOnly]
    private void DespawnObject(GameObject obj, int objectNetId, bool isRespawn = false)
    {
        // if (!obj) return;
        //
        // if (obj.TryGetComponent<GrabbableObject>(out var grabbableObject))
        // {
        //     if (grabbableObject.isHeld && grabbableObject.playerHeldBy is not null)
        //     {
        //         Imperium.PlayerManager.DropItem(new DropItemRequest
        //         {
        //             PlayerId = grabbableObject.playerHeldBy.playerClientId,
        //             ItemIndex = PlayerManager.GetItemHolderSlot(grabbableObject)
        //         });
        //     }
        // }
        // else if (obj.TryGetComponent<SandSpiderAI>(out var sandSpider))
        // {
        //     for (var i = 0; i < sandSpider.webTraps.Count; i++)
        //     {
        //         sandSpider.BreakWebServerRpc(i, (int)objectNetId);
        //     }
        // }
        //
        // try
        // {
        //     if (obj.TryGetComponent<NetworkObject>(out var networkObject)) networkObject.Despawn();
        // }
        // finally
        // {
        //     Destroy(obj);
        //
        //     if (!isRespawn) objectsChangedEvent.DispatchToClients();
        // }
    }

    [ImpAttributes.LocalMethod]
    private static void OnSteamValveBurst(int valveNetId)
    {
        // var steamValve = Imperium.ObjectManager.CurrentLevelObjects[valveNetId].GetComponent<SteamValveHazard>();
        // steamValve.valveHasBurst = true;
        // steamValve.valveHasBeenRepaired = false;
        // steamValve.BurstValve();
    }

    #endregion
}