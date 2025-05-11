#region

using System.Collections.Generic;
using System.Linq;
using Librarium.Binding;
using UnityEngine;

#endregion

namespace Imperium.API.Types;

/// <summary>
///     Defines a custom visualization manager for a gizmo or an indicator in Imperium.
/// </summary>
/// <typeparam name="T">The type of objects this visualizer will handle</typeparam>
/// <typeparam name="R">The visualizer object this manager works with</typeparam>
public abstract class BaseVisualizer<T, R> where R : Component
{
    protected Transform parent { get; private set; }
    protected readonly Dictionary<int, R> visualizerObjects = [];

    private readonly IBinding<bool> visibilityBinding;

    protected BaseVisualizer(Transform parent, IBinding<T> objectsBinding = null, IBinding<bool> visibilityBinding = null)
    {
        this.parent = parent;
        this.visibilityBinding = visibilityBinding;

        if (objectsBinding != null) objectsBinding.OnUpdate += OnObjectBindingUpdate;
        if (visibilityBinding != null) visibilityBinding.OnUpdate += OnVisibilityUpdate;
    }

    private void OnObjectBindingUpdate(T objects)
    {
        OnRefresh(objects);
        if (visibilityBinding != null) OnVisibilityUpdate(visibilityBinding.Value);
    }

    /// <summary>
    ///     Called when the visibility binding is updated.
    /// </summary>
    private void OnVisibilityUpdate(bool isVisible)
    {
        foreach (var obj in visualizerObjects.Values.Where(obj => obj)) obj.gameObject.SetActive(isVisible);
    }

    /// <summary>
    ///     Called when the objects binding is updated.
    /// </summary>
    /// <param name="objects"></param>
    protected virtual void OnRefresh(T objects)
    {
    }

    /// <summary>
    ///     Internal function to clear all the current visualizer objects.
    /// </summary>
    protected void ClearObjects()
    {
        foreach (var indicatorObject in visualizerObjects.Values.Where(indicator => indicator))
        {
            Object.Destroy(indicatorObject.gameObject);
        }

        visualizerObjects.Clear();
    }
}