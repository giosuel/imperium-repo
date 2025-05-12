#region

using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Imperium.API.Types;
using Imperium.Visualizers.Objects;
using Librarium.Binding;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.Visualizers;

/// <summary>
///     Entity-specific gizmos like LoS indicators, target rays, noise rays, etc.
/// </summary>
internal class EnemyGizmos : BaseVisualizer<IReadOnlyCollection<EnemyParent>, EnemyGizmo>
{
    internal readonly Dictionary<string, EntityGizmoConfig> EntityInfoConfigs = [];

    private readonly ConfigFile config;

    internal EnemyGizmos(
        Transform parent,
        IBinding<IReadOnlyCollection<EnemyParent>> objectsBinding,
        IBinding<bool> isArenaLoaded,
        ConfigFile config
    ) : base(parent, objectsBinding)
    {
        this.config = config;

        foreach (var entity in Imperium.ObjectManager.LoadedEntities.Value)
        {
            EntityInfoConfigs[entity.EnemyName] = new EntityGizmoConfig(entity.EnemyName, config);
        }

        isArenaLoaded.OnTrigger += () => HardRefresh(objectsBinding.Value);
    }

    protected override void OnRefresh(IReadOnlyCollection<EnemyParent> objects)
    {
        foreach (var entity in objects.Where(entity => entity))
        {
            if (visualizerObjects.ContainsKey(entity.GetInstanceID())) continue;

            if (!EntityInfoConfigs.TryGetValue(entity.enemyName, out var entityConfig))
            {
                entityConfig = new EntityGizmoConfig(entity.enemyName, config);
                EntityInfoConfigs[entity.enemyName] = entityConfig;
            }

            var entityGizmoObject = new GameObject($"ImpVis_EnemyGizmo_{entity.GetInstanceID()}");
            entityGizmoObject.transform.SetParent(parent);

            var entityGizmo = entityGizmoObject.AddComponent<EnemyGizmo>();
            entityGizmo.Init(entityConfig, entity);

            visualizerObjects[entity.GetInstanceID()] = entityGizmo;
        }
    }

    private void HardRefresh(IReadOnlyCollection<EnemyParent> objects)
    {
        ClearObjects();
        OnRefresh(objects);
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