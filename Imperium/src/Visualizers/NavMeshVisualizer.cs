#region

using Imperium.API.Types;
using Imperium.Util;
using Librarium;
using Librarium.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class NavMeshVisualizer(
    Transform parent,
    ImpBinding<bool> isLoadedBinding,
    ImpBinding<bool> visibilityBinding
) : BaseVisualizer<bool, Component>(parent, isLoadedBinding, visibilityBinding: visibilityBinding)
{
    protected override void OnRefresh(bool isSceneLoaded)
    {
        ClearObjects();

        var index = 0;
        foreach (var navmeshSurface in Geometry.GetNavmeshSurfaces())
        {
            var navmeshVisualizer = new GameObject($"ImpVis_NavMeshSurface_{index}");
            navmeshVisualizer.transform.SetParent(parent);

            var navmeshRenderer = navmeshVisualizer.AddComponent<MeshRenderer>();
            navmeshRenderer.material = ImpAssets.NavmeshMaterial;
            var navmeshFilter = navmeshVisualizer.AddComponent<MeshFilter>();
            navmeshFilter.mesh = navmeshSurface;
            navmeshVisualizer.AddComponent<MeshDataBuilder>();

            visualizerObjects[navmeshVisualizer.GetInstanceID()] = navmeshRenderer;

            index++;
        }
    }
}