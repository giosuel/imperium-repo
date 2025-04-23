#region

using System.Collections.Generic;
using UnityEngine;

#endregion

namespace Imperium.API.Types;

public delegate IEnumerable<GameObject> VisualizerFunc(GameObject target, float size, Material material = null);

public record VisualizerDefinition(
    string identifier,
    VisualizerIdentifier type,
    float size,
    VisualizerFunc visualizer,
    Material material
);

public enum VisualizerIdentifier
{
    Tag,
    Layer,
    Component
}