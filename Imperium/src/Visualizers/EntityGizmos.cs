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
                entityGizmo.Init(entityConfig, entity);

                visualizerObjects[entity.GetInstanceID()] = entityGizmo;
            }
        }
    }

    internal void VisionUpdate(EnemyVision vision)
    {
        if (vision?.Enemy == null) return;

        if (visualizerObjects.TryGetValue(vision.Enemy.EnemyParent.GetInstanceID(), out var entityGizmo))
        {
            entityGizmo.VisionUpdate();
        }
    }

    internal void NoiseVisualizerUpdate(EnemyStateInvestigate instance, Vector3 origin)
    {
        if (visualizerObjects.TryGetValue(instance.Enemy.EnemyParent.GetInstanceID(), out var entityGizmo))
        {
            entityGizmo.NoiseUpdate(origin);
        }
    }
}