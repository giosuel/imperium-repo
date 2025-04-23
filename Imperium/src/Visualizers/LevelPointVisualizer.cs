#region

using System;
using System.Collections.Generic;
using Imperium.API.Types;
using Imperium.Util;
using Imperium.Visualizers.Objects;
using Librarium;
using Librarium.Binding;
using Unity.VisualScripting;
using UnityEngine;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.Visualizers;

internal class LevelPointVisualizer(
    Transform parent,
    ImpBinding<bool> isLoadedBinding,
    ImpBinding<bool> visibilityBinding
) : BaseVisualizer<bool, Component>(parent, isLoadedBinding, visibilityBinding: visibilityBinding)
{
    protected override void OnRefresh(bool isSceneLoaded)
    {
        ClearObjects();

        var levelPointParent = new GameObject($"ImpVis_LevelPoints");
        levelPointParent.transform.SetParent(parent);

        var connections = new HashSet<ValueTuple<Vector3, Vector3>>();

        foreach (var levelPoint in Object.FindObjectsByType<LevelPoint>(FindObjectsSortMode.None))
        {
            var levelPointIndicator = Object.Instantiate(ImpAssets.LevelPoint, levelPointParent.transform);
            levelPointIndicator.transform.localScale = Vector3.one * 0.14f;
            levelPointIndicator.transform.position = levelPoint.transform.position;
            levelPointIndicator.transform.Find("point").AddComponent<LevelPointObject>();
            visualizerObjects.Add(levelPointIndicator.GetInstanceID(), levelPointIndicator.transform);

            foreach (var connectedPoint in levelPoint.ConnectedPoints)
            {
                var connection = new ValueTuple<Vector3, Vector3>(
                    levelPoint.transform.position, connectedPoint.transform.position
                );
                if (connections.Contains(connection)) continue;

                var connectionLine = Geometry.CreateLine(
                    levelPointParent.transform,
                    useWorldSpace: true,
                    startColor: new Color(0.06f, 1f, 0.64f),
                    endColor: new Color(0.06f, 1f, 0.64f),
                    thickness: 0.025f,
                    positions: [connection.Item1 + Vector3.up * 0.04f, connection.Item2 + Vector3.up * 0.04f]
                );

                visualizerObjects.Add(connectionLine.GetInstanceID(), connectionLine.transform);
                connections.Add(connection);
            }
        }
    }
}