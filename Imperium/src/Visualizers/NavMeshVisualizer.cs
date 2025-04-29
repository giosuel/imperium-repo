#region

using Imperium.API.Types;
using Imperium.Util;
using Librarium;
using Librarium.Binding;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class NavMeshVisualizer : BaseVisualizer<bool, Component>
{
    private readonly GameObject navMeshParent;

    internal NavMeshVisualizer(
        Transform parent, ImpBinding<bool> isLoadedBinding, ImpBinding<bool> visibilityBinding
    ) : base(parent, isLoadedBinding, visibilityBinding: visibilityBinding)
    {
        navMeshParent = new GameObject($"ImpVis_NavMesh");
        navMeshParent.transform.SetParent(parent);
    }

    protected override void OnRefresh(bool isSceneLoaded)
    {
        ClearObjects();

        var index = 0;
        foreach (var navmeshSurface in Geometry.GetNavmeshSurfaces())
        {
            var navmeshVisualizer = new GameObject($"ImpVis_NavMeshSurface_{index}");
            navmeshVisualizer.transform.SetParent(navMeshParent.transform);

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