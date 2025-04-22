#region

using Imperium.Core.Scripts;
using Imperium.Util;
using Imperium.Visualizers.Objects;
using Librarium;
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
        var noiseOverlayObj = Instantiate(ImpAssets.NoiseOverlay, transform);
        var noiseOverlayCanvas = noiseOverlayObj.GetComponent<Canvas>();
        noiseOverlayCanvas.sortingOrder = 12;

        var template = noiseOverlayObj.transform.Find("Indicator").gameObject;
        template.gameObject.SetActive(false);

        for (var i = 0; i < noiseIndicatorCount; i++)
        {
            var noiseIndicatorObj = Instantiate(template, noiseOverlayCanvas.transform);
            noiseIndicatorObj.transform.SetParent(noiseOverlayCanvas.transform);

            var noiseIndicator = noiseIndicatorObj.AddComponent<NoiseIndicator>();
            noiseIndicator.Init(noiseOverlayCanvas, transform);

            noiseIndicators[i] = noiseIndicator;
        }

        Imperium.Settings.Visualization.NoiseIndicators.onUpdate += value =>
        {
            if (!value)
            {
                for (var i = 0; i < noiseIndicatorCount; i++)
                {
                    noiseIndicators[i].Deactivate();
                }
            }
        };
    }

    internal void ToggleSpheres(bool isShown)
    {
        foreach (var indicator in noiseIndicators) indicator.ToggleSphere(isShown);
    }
    
    internal void AddNoise(Vector3 position, float radius)
    {
        if (!Imperium.Settings.Visualization.NoiseIndicators.Value) return;

        noiseIndicators[noiseIndex].Activate(position, radius, noiseIndicatorDisplayTime);
        noiseIndex = (noiseIndex + 1) % noiseIndicators.Length;
    }
}