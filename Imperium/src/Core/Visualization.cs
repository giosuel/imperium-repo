#region

using BepInEx.Configuration;
using Imperium.Core.Lifecycle;
using Imperium.Core.Scripts;
using Imperium.Visualizers;
using UnityEngine;

#endregion

namespace Imperium.Core;

internal class Visualization
{
    internal readonly StaticVisualizers StaticVisualizers;

    // internal readonly PlayerGizmos PlayerGizmos;
    internal readonly EnemyGizmos EnemyGizmos;

    internal readonly ObjectInsights ObjectInsights;
    internal readonly NoiseIndicators NoiseIndicators;
    internal readonly NavMeshVisualizer NavMeshVisualizer;
    internal readonly LevelPointVisualizer LevelPointVisualizer;

    internal Visualization(Transform parent, ObjectManager objectManager, ConfigFile config)
    {
        var visualizerParent = new GameObject("Imp_Visualization");
        visualizerParent.transform.SetParent(parent);

        NavMeshVisualizer = new NavMeshVisualizer(
            visualizerParent.transform,
            Imperium.IsArenaLoaded,
            Imperium.Settings.Visualization.NavMeshSurfaces
        );
        LevelPointVisualizer = new LevelPointVisualizer(
            visualizerParent.transform,
            Imperium.IsArenaLoaded,
            Imperium.Settings.Visualization.LevelPoints
        );

        // PlayerGizmos = new PlayerGizmos(visualizerParent.transform, objectManager.CurrentPlayers, config);
        EnemyGizmos = new EnemyGizmos(
            visualizerParent.transform,
            objectManager.CurrentLevelEntities,
            Imperium.IsArenaLoaded,
            config
        );
        ObjectInsights = new ObjectInsights(visualizerParent.transform, config);
        NoiseIndicators = ImpScript.Create<NoiseIndicators>(visualizerParent.transform);
        StaticVisualizers = ImpScript.Create<StaticVisualizers>(visualizerParent.transform);

        Imperium.IsArenaLoaded.OnTrigger += ObjectInsights.Refresh;
        Imperium.IsArenaLoaded.OnTrigger += () => StaticVisualizers.Refresh(true);

        Imperium.ObjectManager.CurrentLevelObjectsChanged += ObjectInsights.Refresh;
        Imperium.ObjectManager.CurrentLevelObjectsChanged += () => StaticVisualizers.Refresh();
    }

    internal void ClearObjects()
    {
        NoiseIndicators.Clear();
        StaticVisualizers.Refresh();
    }
}