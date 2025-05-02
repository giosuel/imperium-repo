#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Imperium.API.Types;

public delegate IEnumerable<GameObject> VisualizerFunc(GameObject target, float size, Material material = null,
    bool overrideInactive = false);

public record VisualizerDefinition(
    string identifier,
    VisualizerIdentifier type,
    float size,
    VisualizerFunc visualizer,
    Material material,
    bool overrideInactive
);

public enum VisualizerIdentifier
{
    Tag,
    Layer,
    Component
}