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

    private readonly GameObject[] noiseSpheres = new GameObject[noiseIndicatorCount];
    private readonly NoiseIndicator[] noiseIndicators = new NoiseIndicator[noiseIndicatorCount];
    private int noiseIndex;

    private void Awake()
    {
        var canvas = Instantiate(ImpAssets.NoiseOverlay, transform);
        var template = canvas.transform.Find("Indicator");
        template.gameObject.SetActive(false);

        for (var i = 0; i < noiseIndicatorCount; i++)
        {
            var noiseIndicatorObj = Instantiate(template, canvas.transform);
            var noiseIndicator = noiseIndicatorObj.gameObject.AddComponent<NoiseIndicator>();
            noiseIndicator.Init(canvas.GetComponent<Canvas>());
            noiseIndicators[i] = noiseIndicator;
        }

        for (var i = 0; i < noiseIndicatorCount; i++)
        {
            noiseSpheres[i] = ImpGeometry.CreatePrimitive(
                PrimitiveType.Sphere,
                transform,
                ImpAssets.FresnelGreen
            );
        }

        Imperium.Settings.Visualization.NoiseIndicators.onUpdate += value =>
        {
            if (!value)
            {
                for (var i = 0; i < noiseIndicatorCount; i++)
                {
                    noiseSpheres[i].SetActive(false);
                    noiseIndicators[i].Deactivate();
                }
            }
        };
    }


    internal void AddNoise(Vector3 position, float radius)
    {
        if (!Imperium.Settings.Visualization.NoiseIndicators.Value) return;

        noiseIndicators[noiseIndex].Activate(position);
        noiseIndex = (noiseIndex + 1) % noiseIndicators.Length;

        noiseSpheres[noiseIndex].transform.position = position;
        noiseSpheres[noiseIndex].transform.localScale = Vector3.one * radius;
        noiseSpheres[noiseIndex].SetActive(true);
    }
}