using BepInEx.Configuration;
using Imperium.Types;
using Imperium.Util;
using Librarium;
using Librarium.Binding;

namespace Imperium.Core.Settings;

internal class VisualizationSettings(ConfigFile config, IBinding<bool> isSceneLoaded, IBinding<bool> isEnabled)
    : SettingsContainer(config)
{
    /// <summary>
    ///     Visualization preferences
    /// </summary>
    internal readonly ImpConfig<bool> SmoothAnimations = new(
        config,
        "Visualization.Visualizers",
        "SmoothAnimations",
        true
    );

    internal readonly ImpConfig<bool> SSAlwaysOnTop = new(
        config,
        "Visualization.Insights",
        "AlwaysOnTop",
        true
    );

    internal readonly ImpConfig<bool> SSAutoScale = new(
        config,
        "Visualization.Insights",
        "AutoScale",
        true
    );

    internal readonly ImpConfig<bool> SSHideInactive = new(
        config,
        "Visualization.Insights",
        "HideInactive",
        false
    );

    internal readonly ImpConfig<float> SSOverlayScale = new(
        config,
        "Visualization.Insights",
        "OverlayScale",
        1
    );

    /// <summary>
    ///     Colliders
    /// </summary>
    internal readonly ImpConfig<bool> Employees = new(
        config,
        "Visualization.Colliders",
        "Employees",
        false,
        value => Imperium.Visualization.Collider(value, "Player", IdentifierType.TAG)
    );

    internal readonly ImpConfig<bool> Entities = new(
        config,
        "Visualization.Colliders",
        "Entities",
        false,
        value => Imperium.Visualization.Objects<PhysGrabObjectBoxCollider>(value, ImpAssets.WireframeCyan)
    );

    internal readonly ImpConfig<bool> MapHazards = new(
        config,
        "Visualization.Colliders",
        "MapHazards",
        false,
        value => Imperium.Visualization.Collider(value, "MapHazards", IdentifierType.LAYER)
    );

    internal readonly ImpConfig<bool> Props = new(
        config,
        "Visualization.Colliders",
        "Props",
        false,
        value => Imperium.Visualization.Collider(value, "PhysicsProp", IdentifierType.TAG)
    );

    internal readonly ImpConfig<bool> Foliage = new(
        config,
        "Visualization.Colliders",
        "Foliage",
        false,
        value => Imperium.Visualization.Collider(value, "EnemySpawn", IdentifierType.LAYER)
    );

    internal readonly ImpConfig<bool> InteractTriggers = new(
        config,
        "Visualization.Colliders",
        "InteractTriggers",
        false,
        value => Imperium.Visualization.Collider(value, "InteractTrigger", IdentifierType.TAG)
    );

    internal readonly ImpConfig<bool> VainShrouds = new(
        config,
        "Visualization.Colliders",
        "VainShrouds",
        false,
        value => Imperium.Visualization.Collider(value, "MoldSpore", IdentifierType.TAG)
    );

    internal readonly ImpConfig<bool> MoldAttractionPoints = new(
        config,
        "Visualization.Colliders",
        "MoldAttractionPoints",
        false,
        value => Imperium.Visualization.Point(value, "MoldAttractionPoint", IdentifierType.TAG)
    );

    internal readonly ImpConfig<bool> LineOfSight = new(
        config,
        "Visualization.Colliders",
        "LineOfSight",
        false,
        value => Imperium.Visualization.Collider(value, "LineOfSight", IdentifierType.LAYER)
    );

    internal readonly ImpConfig<bool> TileBorders = new(
        config,
        "Visualization.Colliders",
        "TileBorders",
        false,
        value => Imperium.Visualization.Collider(value, "Ignore Raycast", IdentifierType.LAYER)
        // value => Imperium.Visualization.Objects<Tile>(value, Imperium.Visualization.VisualizeTileBounds)
    );

    internal readonly ImpConfig<bool> Room = new(
        config,
        "Visualization.Colliders",
        "Room",
        false,
        value => Imperium.Visualization.Collider(value, "Room", IdentifierType.LAYER)
    );

    internal readonly ImpConfig<bool> Colliders = new(
        config,
        "Visualization.Colliders",
        "Visualization.Colliders",
        false,
        value => Imperium.Visualization.Collider(
            value, "Visualization.Colliders", IdentifierType.LAYER, material: ImpAssets.TriggerMaterial
        )
    );

    internal readonly ImpConfig<bool> Triggers = new(
        config,
        "Visualization.Colliders",
        "Triggers",
        false,
        value => Imperium.Visualization.Collider(
            value, "Triggers", IdentifierType.LAYER, material: ImpAssets.TriggerMaterial
        )
    );

    internal readonly ImpConfig<bool> PhysicsObject = new(
        config,
        "Visualization.Colliders",
        "PhysicsObject",
        false,
        value => Imperium.Visualization.Collider(value, "PhysicsObject", IdentifierType.LAYER)
    );

    internal readonly ImpConfig<bool> RoomLight = new(
        config,
        "Visualization.Colliders",
        "RoomLight",
        false,
        value => Imperium.Visualization.Collider(value, "RoomLight", IdentifierType.LAYER)
    );

    internal readonly ImpConfig<bool> Anomaly = new(
        config,
        "Visualization.Colliders",
        "Anomaly",
        false,
        value => Imperium.Visualization.Collider(value, "Anomaly", IdentifierType.LAYER)
    );

    internal readonly ImpConfig<bool> Railing = new(
        config,
        "Visualization.Colliders",
        "Railing",
        false,
        value => Imperium.Visualization.Collider(value, "Railing", IdentifierType.LAYER)
    );

    internal readonly ImpConfig<bool> PlacementBlocker = new(
        config,
        "Visualization.Colliders",
        "PlacementBlocker",
        false,
        value => Imperium.Visualization.Collider(value, "PlacementBlocker", IdentifierType.LAYER)
    );

    internal readonly ImpConfig<bool> Terrain = new(
        config,
        "Visualization.Colliders",
        "Terrain",
        false,
        value => Imperium.Visualization.Collider(value, "Terrain", IdentifierType.LAYER)
    );

    internal readonly ImpConfig<bool> PlaceableShipObjects = new(
        config,
        "Visualization.Colliders",
        "PlaceableShipObjects",
        false,
        value => Imperium.Visualization.Collider(value, "PlaceableShipObjects", IdentifierType.LAYER)
    );

    internal readonly ImpConfig<bool> MiscLevelGeometry = new(
        config,
        "Visualization.Colliders",
        "MiscLevelGeometry",
        false,
        value => Imperium.Visualization.Collider(value, "MiscLevelGeometry", IdentifierType.LAYER)
    );

    internal readonly ImpConfig<bool> ScanNode = new(
        config,
        "Visualization.Colliders",
        "ScanNode",
        false,
        value => Imperium.Visualization.Collider(value, "ScanNode", IdentifierType.LAYER)
    );

    /// <summary>
    ///     Overlays
    /// </summary>
    internal readonly ImpConfig<bool> Vents = new(
        config,
        "Visualization.Overlays",
        "Vents",
        false,
        value => Imperium.Visualization.Point(
            value,
            "EnemySpawn",
            IdentifierType.TAG,
            material: ImpAssets.XRay
        )
    );

    internal readonly ImpConfig<bool> AINodesIndoor = new(
        config,
        "Visualization.Overlays",
        "AINodesIndoor",
        false,
        value => Imperium.Visualization.Point(
            value,
            "AINode",
            IdentifierType.TAG,
            size: 0.5f,
            material: ImpAssets.FresnelWhite
        )
    );

    internal readonly ImpConfig<bool> AINodesOutdoor = new(
        config,
        "Visualization.Overlays",
        "AINodesOutdoor",
        false,
        value => Imperium.Visualization.Point(
            value,
            "OutsideAINode",
            IdentifierType.TAG,
            size: 0.8f,
            material: ImpAssets.FresnelWhite
        )
    );

    internal readonly ImpConfig<bool> AINodesCave = new(
        config,
        "Visualization.Overlays",
        "CaveNode",
        false,
        value => Imperium.Visualization.Point(
            value,
            "CaveNode",
            IdentifierType.TAG,
            size: 0.8f,
            material: ImpAssets.FresnelWhite
        )
    );

    internal readonly ImpConfig<bool> SpawnDenialPoints = new(
        config,
        "Visualization.Overlays",
        "SpawnDenialPoints",
        false,
        value => Imperium.Visualization.Point(
            value,
            "SpawnDenialPoint",
            IdentifierType.TAG,
            size: 16,
            material: ImpAssets.FresnelRed
        )
    );

    internal readonly ImpConfig<bool> BeeSpawns = new(
        config,
        "Visualization.Overlays",
        "BeeSpawns",
        false,
        value => Imperium.Visualization.Point(
            value,
            "OutsideAINode",
            IdentifierType.TAG,
            size: 20f,
            material: ImpAssets.FresnelYellow
        )
    );

    internal readonly ImpConfig<bool> OutsideEntitySpawns = new(
        config,
        "Visualization.Overlays",
        "OutsideEntitySpawns",
        false,
        value => Imperium.Visualization.Point(
            value,
            "OutsideAINode",
            IdentifierType.TAG,
            size: 10f,
            material: ImpAssets.FresnelGreen
        )
    );

    internal readonly ImpConfig<bool> NavMeshSurfaces = new(
        config,
        "Visualization.Overlays",
        "NavMeshSurfaces",
        false
    );

    /// <summary>
    ///     Gizmos
    /// </summary>
    internal readonly ImpConfig<bool> SpawnIndicators = new(
        config,
        "Visualization.Gizmos",
        "SpawnIndicators",
        false
    );

    internal readonly ImpConfig<bool> VentTimers = new(
        config,
        "Visualization.Gizmos",
        "VentTimers",
        false
    );

    internal readonly ImpConfig<bool> NoiseIndicators = new(
        config,
        "Visualization.Gizmos",
        "NoiseIndicators",
        false
    );

    internal readonly ImpConfig<bool> ScrapSpawns = new(
        config,
        "Visualization.Gizmos",
        "ScrapSpawns",
        false
    );

    internal readonly ImpConfig<bool> HazardSpawns = new(
        config,
        "Visualization.Gizmos",
        "HazardSpawns",
        false
    );

    internal readonly ImpConfig<bool> ShotgunIndicators = new(
        config,
        "Visualization.Gizmos",
        "ShotgunIndicators",
        false
    );

    internal readonly ImpConfig<bool> ShovelIndicators = new(
        config,
        "Visualization.Gizmos",
        "ShovelIndicators",
        false
    );

    internal readonly ImpConfig<bool> KnifeIndicators = new(
        config,
        "Visualization.Gizmos",
        "KnifeIndicators",
        false
    );

    internal readonly ImpConfig<bool> LandmineIndicators = new(
        config,
        "Visualization.Gizmos",
        "LandmineIndicators",
        false
    );

    internal readonly ImpConfig<bool> SpikeTrapIndicators = new(
        config,
        "Visualization.Gizmos",
        "SpikeTrapIndicators",
        false
    );
}