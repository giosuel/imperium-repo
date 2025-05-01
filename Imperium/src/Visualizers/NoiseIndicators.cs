#region

using Imperium.Core.Scripts;
using Imperium.Visualizers.Objects;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class NoiseIndicators : ImpScript
{
    private const int noiseIndicatorCount = 20;
    private const int noiseIndicatorDisplayTime = 10;

    private readonly NoiseIndicator[] noiseIndicators = new NoiseIndicator[noiseIndicatorCount];
    private int noiseIndex;

    private void Awake()
    {
        for (var i = 0; i < noiseIndicatorCount; i++)
        {
            var noiseIndicatorObj = new GameObject("ImpVis_NoiseIndicator" + i);
            noiseIndicatorObj.transform.SetParent(transform);

            noiseIndicators[i] = noiseIndicatorObj.AddComponent<NoiseIndicator>();
        }

        Imperium.Settings.Visualization.NoiseIndicators.onUpdate += value =>
        {
            if (!value)
            {
                for (var i = 0; i < noiseIndicatorCount; i++) noiseIndicators[i].Deactivate();
            }
        };
    }

    internal void Clear()
    {
        foreach (var indicator in noiseIndicators) indicator.Deactivate();
    }

    internal void AddNoise(Vector3 position, float radius, bool isMuted)
    {
        if (!Imperium.Settings.Visualization.NoiseIndicators.Value
            || !Imperium.IsArenaLoaded
            || Imperium.GameManager.IsGameLoading)
        {
            return;
        }

        noiseIndicators[noiseIndex].Activate(position, radius, noiseIndicatorDisplayTime, isMuted);
        noiseIndex = (noiseIndex + 1) % noiseIndicators.Length;
    }
}