#region

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

internal class LevelPointVisualizer : BaseVisualizer<bool, Component>
{
    private readonly GameObject levelPointParent;

    internal LevelPointVisualizer(
        Transform parent, ImpBinding<bool> isLoadedBinding, ImpBinding<bool> visibilityBinding
    ) : base(parent, isLoadedBinding, visibilityBinding)
    {
        levelPointParent = new GameObject("ImpVis_LevelPoints");
        levelPointParent.transform.SetParent(parent);
    }

    private readonly Color invalidColor = new(1f, 0.47f, 0.07f);
    private readonly Color validColor = new(0.06f, 1f, 0.64f);

    protected override void OnRefresh(bool isSceneLoaded)
    {
        ClearObjects();

        if (!isSceneLoaded) return;

        var connections = new Dictionary<Edge, LineRenderer>();
        foreach (var levelPoint in Object.FindObjectsByType<LevelPoint>(FindObjectsSortMode.None))
        {
            var levelPointIndicator = Object.Instantiate(ImpAssets.LevelPoint, levelPointParent.transform);
            levelPointIndicator.transform.localScale = Vector3.one * 0.14f;
            levelPointIndicator.transform.position = levelPoint.transform.position;
            levelPointIndicator.transform.Find("point").AddComponent<LevelPointObject>();
            visualizerObjects.Add(levelPointIndicator.GetInstanceID(), levelPointIndicator.transform);

            foreach (var connectedPoint in levelPoint.ConnectedPoints)
            {
                var connection = new Edge(levelPoint.transform.position, connectedPoint.transform.position);
                if (!connections.TryGetValue(connection, out var connectionLine))
                {
                    connectionLine = Geometry.CreateLine(
                        levelPointParent.transform,
                        useWorldSpace: true,
                        startColor: invalidColor,
                        endColor: invalidColor,
                        thickness: 0.025f,
                        positions: [connection.PointA + Vector3.up * 0.04f, connection.PointB + Vector3.up * 0.04f]
                    );

                    visualizerObjects.Add(connectionLine.GetInstanceID(), connectionLine.transform);
                    connections[connection] = connectionLine;
                }
                else
                {
                    Geometry.SetLineColor(connectionLine, validColor, validColor);
                }
            }
        }
    }
}