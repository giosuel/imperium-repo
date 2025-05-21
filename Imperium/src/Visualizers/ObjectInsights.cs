#region

using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Imperium.API.Types;
using Imperium.Types;
using Imperium.Visualizers.Objects;
using Librarium;
using Librarium.Binding;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.Visualizers;

/// <summary>
///     Screen-space overlays containing custom insights of objects (e.g. Health, Behaviour State, Movement Speed, etc.)
/// </summary>
internal class ObjectInsights : BaseVisualizer<HashSet<Component>, ObjectInsight>
{
    private readonly ConfigFile config;

    internal readonly ImpBinding<Dictionary<Type, ImpBinding<bool>>> InsightVisibilityBindings = new([]);

    // internal readonly ImpConfig<bool> CustomInsights;

    // Holds all the logical insights, per-type
    private readonly ImpBinding<Dictionary<Type, InsightDefinition<Component>>> registeredInsights = new([]);

    private readonly HashSet<int> insightVisualizerObjects = [];

    internal ObjectInsights(Transform parent, ConfigFile config) : base(parent)
    {
        this.config = config;

        // CustomInsights = new ImpConfig<bool>(
        //     config,
        //     "Visualization.Insights", "Custom", false
        // );

        RegisterDefaultInsights();
        Refresh();

        foreach (var (_, binding) in InsightVisibilityBindings.Value) binding.OnTrigger += Refresh;

        registeredInsights.OnTrigger += Refresh;
    }

    internal void Refresh()
    {
        if (!Imperium.IsLevelLoaded) return;

        // Skip udpating if no insights are visible
        if (InsightVisibilityBindings.Value.All(binding => !binding.Value.Value)) return;

        foreach (var obj in Object.FindObjectsOfType<GameObject>())
        {
            // Make sure visualizers aren't being added to other visualization objects
            if (insightVisualizerObjects.Contains(obj.GetInstanceID())) continue;

            foreach (var component in obj.GetComponents<Component>().Where(component => component))
            {
                var typeInsight = FindMostMatchingInsightDefinition(component.GetType());
                if (typeInsight != null)
                {
                    if (!visualizerObjects.TryGetValue(component.GetInstanceID(), out var objectInsight))
                    {
                        var objectInsightObject = new GameObject($"Imp_ObjectInsight_{obj.GetInstanceID()}");
                        objectInsightObject.transform.SetParent(obj.transform);
                        insightVisualizerObjects.Add(objectInsightObject.GetInstanceID());

                        objectInsight = objectInsightObject.AddComponent<ObjectInsight>();

                        // Find the type-specific config, or use the custom one if the specific can't be found
                        objectInsight.Init(component, typeInsight);

                        visualizerObjects[component.GetInstanceID()] = objectInsight;
                    }
                    else if (typeInsight != objectInsight.InsightDefinition)
                    {
                        // Update the insight definition if a more specific one was found
                        objectInsight.UpdateInsightDefinition(typeInsight);
                    }
                }
            }
        }
    }

    internal InsightDefinition<T> InsightsFor<T>() where T : Component
    {
        if (registeredInsights.Value.TryGetValue(typeof(T), out var insightsDefinition))
        {
            return insightsDefinition as InsightDefinition<T>;
        }

        var newInsightDefinition = new InsightDefinitionImpl<T>(
            registeredInsights.Value, InsightVisibilityBindings, config
        );
        registeredInsights.Value[typeof(T)] = newInsightDefinition;
        registeredInsights.Refresh();

        return newInsightDefinition;
    }

    /// <summary>
    ///     Finds the most matching registered object insight definition for a given type.
    /// </summary>
    private InsightDefinition<Component> FindMostMatchingInsightDefinition(Type inputType)
    {
        foreach (var type in Debugging.GetParentTypes(inputType))
        {
            if (registeredInsights.Value.TryGetValue(type, out var typeInsight)) return typeInsight;
        }

        return null;
    }

    private void RegisterDefaultInsights()
    {
        InsightsFor<EnemyParent>()
            .SetNameGenerator(enemy => enemy.enemyName)
            .SetIsDisabledGenerator(enemy => !enemy.Spawned)
            .RegisterInsight("Health", enemy => $"{enemy.Enemy.Health.health} HP")
            .RegisterInsight("Current State", Core.Lifecycle.GameManager.GetEnemyState)
            .RegisterInsight("Spawned Paused", enemy => Formatting.FormatSecondsMinutes(enemy.spawnedTimerPauseTimer))
            .RegisterInsight("Spawned Timer", enemy => Formatting.FormatSecondsMinutes(enemy.SpawnedTimer))
            .RegisterInsight("Despawned Timer", enemy => Formatting.FormatSecondsMinutes(enemy.DespawnedTimer))
            .RegisterInsight("Valuable Timer", enemy => Formatting.FormatSecondsMinutes(enemy.valuableSpawnTimer))
            .RegisterInsight("No Vision Timer",
                enemy => enemy.Enemy.Vision ? Formatting.FormatSecondsMinutes(enemy.Enemy.Vision.DisableTimer) : "?"
            )
            .RegisterInsight("Player Close", enemy => enemy.playerClose.ToString())
            .RegisterInsight("Spawn Idle Timer",
                _ => Formatting.FormatSecondsMinutes(EnemyDirector.instance.spawnIdlePauseTimer)
            )
            .RegisterInsight("Action Timer",
                _ => Formatting.FormatSecondsMinutes(EnemyDirector.instance.enemyActionAmount)
            )
            .SetPositionOverride(enemy => enemy.Enemy.transform.position)
            .SetConfigKey("Enemies");

        InsightsFor<ExtractionPoint>()
            .SetNameGenerator(_ => "Extraction Point")
            .RegisterInsight("Current State", point => point.currentState.ToString())
            .RegisterInsight("Haul Goal", point => SemiFunc.DollarGetString(point.haulGoal))
            .RegisterInsight("Haul Current", point => SemiFunc.DollarGetString(point.haulCurrent))
            .RegisterInsight("In Start Room", point => $"{point.inStartRoom}")
            .SetPositionOverride(point => point.transform.position)
            .SetConfigKey("Extraction Points");

        InsightsFor<ValuableObject>()
            .SetNameGenerator(valuable => valuable.name.Replace("Valuable ", "").Replace("(Clone)", ""))
            .RegisterInsight("Value",
                valuable => $"${SemiFunc.DollarGetString(Mathf.RoundToInt(valuable.dollarValueCurrent))}"
            )
            .RegisterInsight("Is Discovered", valuable => $"{valuable.discovered}")
            .RegisterInsight("Durability", valuable => $"{valuable.durabilityPreset.durability}")
            .RegisterInsight("Discovered Timer", valuable => $"{valuable.discoveredReminderTimer:0}s")
            .SetPositionOverride(valuable => valuable.transform.position)
            .SetConfigKey("Valuables");
    }

    /*
     * Default position override places the insight panel at 3/4 the hight of the object, based on the
     * first child collider found.
     */
    private readonly Dictionary<int, Vector3?> entityColliderCache = [];

    private Vector3 DefaultPositionOverride(Component obj)
    {
        if (!entityColliderCache.TryGetValue(obj.GetInstanceID(), out var colliderCenter))
        {
            colliderCenter = obj.GetComponentInChildren<BoxCollider>()?.center
                             ?? obj.GetComponentInChildren<CapsuleCollider>()?.center;

            entityColliderCache[obj.GetInstanceID()] = colliderCenter;
        }

        return colliderCenter.HasValue
            ? obj.transform.position
              + Vector3.up * colliderCenter.Value.y
                           * obj.transform.localScale.y
                           * 1.5f
            : obj.transform.position;
    }
}