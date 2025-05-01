using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using HarmonyLib;
using Imperium.API.Types;
using Imperium.Core.Scripts;
using Imperium.Types;
using Imperium.Util;
using Librarium;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Imperium.Visualizers;

public class StaticVisualizers : ImpScript
{
    // Contains all registered visualizers indexed by their unique identifier
    private readonly Dictionary<string, VisualizerDefinition> VisualizerRegistry = new();

    // Set of all the unique identifiers of the currently enabled visualizers
    private readonly HashSet<string> EnabledVisualizers = [];

    // Cache that holds all the visualizer game objects (unique identifier -> (target instance ID -> visualizer objects))
    // Note: This dictionary can contain null values if target objects have been destroyed
    private readonly Dictionary<string, Dictionary<int, List<GameObject>>> VisualizerObjectMap = new();

    private static Material DefaultMaterial => ImpAssets.WireframeCyan;

    internal void Collider(
        bool isOn,
        string identifier,
        VisualizerIdentifier type,
        Material material = null,
        bool overrideInactive = false
    ) => Visualize(identifier, isOn, VisualizeColliders, type, 0f, material, overrideInactive);

    internal void Collider<T>(
        bool isOn,
        Material material = null,
        bool overrideInactive = false
    ) => Visualize(typeof(T).AssemblyQualifiedName, isOn, VisualizeColliders, VisualizerIdentifier.Component, 0f, material,
        overrideInactive);

    internal void Point(
        bool isOn,
        string identifier,
        VisualizerIdentifier type,
        float radius = 1,
        Material material = null,
        bool overrideInactive = false
    ) => Visualize(identifier, isOn, VisualizePoint, type, radius, material, overrideInactive);

    internal void Point<T>(
        bool isOn,
        float radius = 1,
        Material material = null,
        bool overrideInactive = false
    ) => Visualize(typeof(T).AssemblyQualifiedName, isOn, VisualizePoint, VisualizerIdentifier.Component, radius, material,
        overrideInactive);

    internal void Refresh(bool hardRefresh = false)
    {
        if (hardRefresh)
        {
            VisualizerObjectMap.Values
                .SelectMany(objs => objs.Values)
                .SelectMany(list => list)
                .Do(Destroy);

            VisualizerObjectMap.Clear();
        }

        foreach (var uniqueIdentifier in VisualizerRegistry.Keys)
        {
            ToggleVisualizer(uniqueIdentifier, EnabledVisualizers.Contains(uniqueIdentifier));
        }
    }

    private void ToggleVisualizer(string uniqueIdentifier, bool isOn)
    {
        if (!VisualizerRegistry.TryGetValue(uniqueIdentifier, out var visualizerDefinition))
        {
            Imperium.IO.LogError($"[VIS] Failed to toggle unregistered visualizer '{uniqueIdentifier}'. Aborting.");
            return;
        }

        if (!isOn)
        {
            EnabledVisualizers.Remove(uniqueIdentifier);
            if (VisualizerObjectMap.TryGetValue(uniqueIdentifier, out var objectDict))
            {
                foreach (var visualizerObjs in objectDict.Values)
                {
                    ImpUtils.ToggleGameObjects(visualizerObjs, false);
                }
            }

            return;
        }

        EnabledVisualizers.Add(uniqueIdentifier);
        if (!VisualizerObjectMap.TryGetValue(uniqueIdentifier, out var visualizerObjects))
        {
            visualizerObjects = [];
            VisualizerObjectMap[uniqueIdentifier] = visualizerObjects;
        }

        // Go through all target objects and toggle / create the visualizers for them
        foreach (var target in GetTargetObjects(visualizerDefinition.identifier, visualizerDefinition.type,
                     visualizerDefinition.overrideInactive))
        {
            if (visualizerObjects.TryGetValue(target.GetInstanceID(), out var targetVisualizerObjects))
            {
                ImpUtils.ToggleGameObjects(targetVisualizerObjects, true);
            }
            else
            {
                visualizerObjects.Add(target.GetInstanceID(), visualizerDefinition.visualizer(
                    target, visualizerDefinition.size, visualizerDefinition.material, visualizerDefinition.overrideInactive
                ).ToList());
            }
        }
    }

    private void Visualize(
        string identifier,
        bool isOn,
        VisualizerFunc visualizer,
        VisualizerIdentifier type,
        float size,
        Material material,
        bool overrideInactive
    )
    {
        var uniqueIdentifier = $"{identifier}_{size}";

        VisualizerRegistry[uniqueIdentifier] = new VisualizerDefinition(
            identifier, type, size, visualizer, material, overrideInactive
        );

        ToggleVisualizer(uniqueIdentifier, isOn);
    }

    private IEnumerable<GameObject> VisualizePoint(
        GameObject target,
        float size,
        Material material = null,
        bool overrideInactive = false
    )
    {
        var parent = overrideInactive ? transform : target.transform;
        var point = Geometry.CreatePrimitive(
            PrimitiveType.Sphere,
            parent,
            material: material ?? DefaultMaterial,
            size,
            name: $"ImpVis_{target.GetInstanceID()}"
        );
        point.transform.position = target.transform.position;

        return [point];
    }

    private static IEnumerable<GameObject> VisualizeColliders(GameObject target, float size, Material material = null,
        bool overrideInactive = false)
    {
        return target.GetComponentsInChildren<BoxCollider>()
            .Select(collider => VisualizeBoxCollider(collider, material))
            .Concat(
                target.GetComponentsInChildren<CapsuleCollider>()
                    .Select(collider => VisualizeCapsuleCollider(collider, material))
            )
            .Concat(
                target.GetComponentsInChildren<SphereCollider>()
                    .Select(collider => VisualizeSphereCollider(collider, material))
            )
            .ToList();
    }

    private static GameObject VisualizeBoxCollider(BoxCollider collider, Material material = null)
    {
        if (!collider) return null;

        var visualizer = Geometry.CreatePrimitive(
            PrimitiveType.Cube,
            collider.transform,
            material ?? DefaultMaterial,
            name: $"ImpVis_{collider.GetInstanceID()}"
        );

        var transform = collider.transform;
        visualizer.transform.position = transform.position;
        visualizer.transform.localPosition = collider.center;
        visualizer.transform.localScale = collider.size;
        visualizer.transform.rotation = transform.rotation;

        return visualizer;
    }

    private static GameObject VisualizeCapsuleCollider(CapsuleCollider collider, Material material = null)
    {
        if (!collider) return null;

        var visualizer = Geometry.CreatePrimitive(
            PrimitiveType.Capsule,
            collider.transform,
            material ?? DefaultMaterial,
            name: $"ImpVis_{collider.GetInstanceID()}"
        );

        visualizer.transform.position = collider.transform.position;
        visualizer.transform.localPosition = collider.center;
        visualizer.transform.localScale = new Vector3(collider.radius * 2, collider.height / 2, collider.radius * 2);
        visualizer.transform.rotation = collider.transform.rotation;

        return visualizer;
    }

    private static GameObject VisualizeSphereCollider(SphereCollider collider, Material material = null)
    {
        if (!collider) return null;

        var visualizer = Geometry.CreatePrimitive(
            PrimitiveType.Sphere,
            collider.transform,
            material ?? DefaultMaterial,
            name: $"ImpVis_{collider.GetInstanceID()}"
        );

        visualizer.transform.position = collider.transform.position;
        visualizer.transform.localPosition = collider.center;
        visualizer.transform.localScale = Vector3.one * collider.radius;
        visualizer.transform.rotation = collider.transform.rotation;

        return visualizer;
    }

    private static IEnumerable<GameObject> GetTargetObjects(
        string identifier, VisualizerIdentifier type, bool overrideInactive
    ) => type switch
    {
        VisualizerIdentifier.Tag => GameObject.FindGameObjectsWithTag(identifier),
        VisualizerIdentifier.Layer => FindObjectsByType<GameObject>(
                findObjectsInactive: overrideInactive ? FindObjectsInactive.Include : FindObjectsInactive.Exclude,
                FindObjectsSortMode.None
            )
            .Where(obj => obj.layer == LayerMask.NameToLayer(identifier))
            .ToArray(),
        VisualizerIdentifier.Component => FindObjectsByType(
            Type.GetType(identifier), FindObjectsSortMode.None
        ).Select(obj => obj switch
            {
                GameObject go => go,
                Component component => component.gameObject,
                _ => null
            }
        ).Where(obj => obj),
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
    };
}