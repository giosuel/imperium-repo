#region

using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Imperium.API.Types;
using Imperium.Types;
using Librarium.Binding;
using Imperium.Visualizers.Objects;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

/// <summary>
///     Entity-specific gizmos like LoS indicators, target rays, noise rays, etc.
/// </summary>
internal class EntityGizmos : BaseVisualizer<IReadOnlyCollection<EnemyParent>, EntityGizmo>
{
    internal readonly Dictionary<string, EntityGizmoConfig> EntityInfoConfigs = [];

    private readonly ConfigFile config;

    internal EntityGizmos(
        Transform parent,
        IBinding<IReadOnlyCollection<EnemyParent>> objectsBinding,
        ConfigFile config
    ) : base(parent, objectsBinding)
    {
        this.config = config;

        foreach (var entity in Imperium.ObjectManager.LoadedEntities.Value)
        {
            EntityInfoConfigs[entity.EnemyName] = new EntityGizmoConfig(entity.EnemyName, config);
        }
    }

    protected override void OnRefresh(IReadOnlyCollection<EnemyParent> objects)
    {
        ClearObjects();

        foreach (var entity in objects.Where(entity => entity))
        {
            if (!visualizerObjects.ContainsKey(entity.GetInstanceID()))
            {
                if (!EntityInfoConfigs.TryGetValue(entity.enemyName, out var entityConfig))
                {
                    entityConfig = new EntityGizmoConfig(entity.enemyName, config);
                    EntityInfoConfigs[entity.enemyName] = entityConfig;
                }

                var entityGizmoObject = new GameObject($"Imp_EntityGizmo_{entity.GetInstanceID()}");
                entityGizmoObject.transform.SetParent(parent);

                var entityGizmo = entityGizmoObject.AddComponent<EntityGizmo>();
                entityGizmo.Init(entityConfig, Imperium.Visualization, entity);

                visualizerObjects[entity.GetInstanceID()] = entityGizmo;
            }
        }
    }

    internal void NoiseVisualizerUpdate(EnemyParent instance, Vector3 origin)
    {
        if (visualizerObjects.TryGetValue(instance.GetInstanceID(), out var entityGizmo))
        {
            entityGizmo.NoiseVisualizerUpdate(origin);
        }
    }

    internal void ConeVisualizerUpdate(
        EnemyParent instance, Transform eye, float angle, float length, Material material,
        GizmoType gizmoType = GizmoType.LineOfSight, GizmoDuration gizmoDuration = GizmoDuration.AIInterval,
        int id = -1,
        Func<Vector3> relativePositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
    )
    {
        if (visualizerObjects.TryGetValue(instance.GetInstanceID(), out var entityGizmo))
        {
            entityGizmo.ConeVisualizerUpdate(
                eye ?? instance.transform,
                angle, length, material,
                visConfig => gizmoType == GizmoType.LineOfSight ? visConfig.LineOfSight : visConfig.Custom,
                gizmoDuration,
                id,
                relativePositionOverride,
                absolutePositionOverride
            );
        }
    }

    internal void SphereVisualizerUpdate(
        EnemyParent instance, Transform eye, float radius, Material material,
        GizmoType gizmoType = GizmoType.LineOfSight, GizmoDuration gizmoDuration = GizmoDuration.AIInterval,
        int id = -1,
        Func<Vector3> relativePositionOverride = null,
        Func<Transform, Vector3> absolutePositionOverride = null
    )
    {
        if (visualizerObjects.TryGetValue(instance.GetInstanceID(), out var entityGizmo))
        {
            entityGizmo.SphereVisualizerUpdate(
                eye ?? instance.transform,
                radius, material,
                visConfig => gizmoType == GizmoType.LineOfSight ? visConfig.LineOfSight : visConfig.Custom,
                gizmoDuration,
                id,
                relativePositionOverride,
                absolutePositionOverride
            );
        }
    }

    internal void StaticSphereVisualizerUpdate(
        EnemyParent instance, Vector3 position, Material material,
        float radius = 2f,
        GizmoType gizmoType = GizmoType.LineOfSight, GizmoDuration gizmoDuration = GizmoDuration.Indefinite,
        int id = -1
    )
    {
        if (visualizerObjects.TryGetValue(instance.GetInstanceID(), out var entityGizmo))
        {
            entityGizmo.StaticSphereVisualizerUpdate(
                instance.gameObject,
                position, radius, material,
                visConfig => gizmoType == GizmoType.LineOfSight ? visConfig.LineOfSight : visConfig.Custom,
                gizmoDuration,
                id
            );
        }
    }
}