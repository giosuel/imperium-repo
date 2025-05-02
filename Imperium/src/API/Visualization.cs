#region

using Imperium.API.Types;
using UnityEngine;

#endregion

namespace Imperium.API;

public static class Visualization
{
    /// <summary>
    ///     Retrieve or create the insight definition for a specific component type.
    ///     Insights are visualizations of instance fields of components in Lethal Company. They are displayed in the insight
    ///     panels that can be toggled per component type. The insight definition holds all information about the insight
    ///     panel.
    ///     Insight definitions support inheritance, meaning components that inherit from another component that already has
    ///     an insight defintions, will inherit from that.
    /// </summary>
    /// <typeparam name="T">The type of component your insight is meant for</typeparam>
    /// <returns>The existing or newly created insight definition for the given type</returns>
    public static InsightDefinition<T> InsightsFor<T>() where T : Component
    {
        APIHelpers.AssertImperiumReady();

        return Imperium.Visualization.ObjectInsights.InsightsFor<T>();
    }

    /// <summary>
    ///     Visualizes the colliders of a group of game objects by tag, layer or object type.
    ///     Can display multiple visualizers per object as long as they have different sizes.
    ///     When picking component as type, the name of the component type is the identifier.
    /// </summary>
    /// <param name="isOn">Whether the visualizer is turned on or off</param>
    /// <param name="identifier">Tag, layer or object type of the target object group</param>
    /// <param name="type">Whether the identifier is a tag, a layer or the name of an object type</param>
    /// <param name="material"></param>
    /// <returns></returns>
    public static void Collider(
        bool isOn,
        string identifier,
        VisualizerIdentifier type,
        Material material = null
    )
    {
        APIHelpers.AssertImperiumReady();

        Imperium.Visualization.StaticVisualizers.Collider(isOn, identifier, type, material);
    }

    /// <summary>
    ///     Visualizes the colliders of a group of game objects of the given type.
    ///     Can display multiple visualizers per object as long as they have different sizes.
    /// </summary>
    /// <param name="isOn">Whether the visualizer is turned on or off</param>
    /// <param name="material">The material to use for the visualizer</param>
    /// <returns></returns>
    public static void Collider<T>(
        bool isOn,
        Material material = null
    )
    {
        APIHelpers.AssertImperiumReady();

        Imperium.Visualization.StaticVisualizers.Collider<T>(isOn, material);
    }

    /// <summary>
    ///     Visualizes a group of game objects with a sphere by tag, layer or object type.
    ///     Can display multiple visualizers per object as long as they have different sizes.
    ///     When picking component as type, the name of the component type is the identifier.
    /// </summary>
    /// <param name="isOn">Whether the visualizer is turned on or off</param>
    /// <param name="identifier">Tag, layer or object type of the target object group</param>
    /// <param name="type">Whether the identifier is a tag, a layer or the name of an object type</param>
    /// <param name="radius">The radius of the visualizer sphere</param>
    /// <param name="material">The material to use for the visualizer</param>
    /// <returns></returns>
    public static void Point(
        bool isOn,
        string identifier,
        VisualizerIdentifier type,
        float radius = 1,
        Material material = null
    )
    {
        APIHelpers.AssertImperiumReady();

        Imperium.Visualization.StaticVisualizers.Point(isOn, identifier, type, radius, material);
    }

    /// <summary>
    ///     Visualizes the colliders of a group of game objects of the given type.
    ///     Can display multiple visualizers per object as long as they have different sizes.
    /// </summary>
    /// <param name="isOn">Whether the visualizer is turned on or off</param>
    /// <param name="radius">The radius of the visualizer sphere</param>
    /// <param name="material">The material to use for the visualizer</param>
    /// <returns></returns>
    public static void Point<T>(
        bool isOn,
        float radius = 1,
        Material material = null
    )
    {
        APIHelpers.AssertImperiumReady();

        Imperium.Visualization.StaticVisualizers.Point<T>(isOn, radius, material);
    }

    /// <summary>
    ///     Refreshes all the currently active static visualizers.
    /// </summary>
    /// <param name="hardRefresh">Whether to destroy and rebuild all static visualizers.</param>
    public static void Refresh(bool hardRefresh = false)
    {
        APIHelpers.AssertImperiumReady();

        Imperium.Visualization.StaticVisualizers.Refresh(hardRefresh);
    }

    /// <summary>
    ///     Clears all transient visualizer objects such as noise indicators and refreshes all static visualizers.
    /// </summary>
    public static void Clear()
    {
        APIHelpers.AssertImperiumReady();

        Imperium.Visualization.ClearObjects();
    }
}