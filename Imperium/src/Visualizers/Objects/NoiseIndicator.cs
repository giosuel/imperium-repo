#region

using System;
using Imperium.Util;
using Librarium;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.Visualizers.Objects;

public class NoiseIndicator : MonoBehaviour
{
    private float timer;
    private float totalTime;
    private float noiseRange;

    private Vector3 worldPosition;
    private bool isDone;
    private TMP_Text indicatorDistanceText;
    private TMP_Text indicatorRangeText;
    private Image indicatorImage;

    private Canvas canvas;
    private RectTransform canvasRect;

    private RectTransform indicatorTransform;
    private RectTransform indicatorArrow;

    private GameObject sphere;

    private readonly Color indicatorColor = new(0.737f, 0.463f, 0.243f);

    internal void Init(Canvas parentCanvas, Transform objectParent)
    {
        canvas = parentCanvas;
        canvasRect = canvas.GetComponent<RectTransform>();

        indicatorDistanceText = transform.Find("Text").GetComponent<TMP_Text>();
        indicatorRangeText = transform.Find("Range").GetComponent<TMP_Text>();
        indicatorImage = transform.Find("Image").GetComponent<Image>();
        indicatorTransform = GetComponent<RectTransform>();
        indicatorArrow = transform.Find("Arrow").GetComponent<RectTransform>();

        indicatorTransform.gameObject.SetActive(false);

        sphere = Geometry.CreatePrimitive(
            PrimitiveType.Sphere,
            objectParent,
            ImpAssets.FresnelGreen
        );
    }

    internal void Activate(Vector3 position, float radius, int time)
    {
        timer = time;
        totalTime = time;
        isDone = false;
        noiseRange = radius;

        worldPosition = position;
        indicatorDistanceText.color = ImpUtils.Interface.ChangeAlpha(indicatorColor, 1);
        indicatorImage.color = ImpUtils.Interface.ChangeAlpha(indicatorColor, 1);
        transform.localScale = Vector3.one * 2;

        sphere.transform.position = position;
        sphere.transform.localScale = Vector3.one * radius;

        gameObject.SetActive(true);
    }

    internal void Deactivate()
    {
        sphere.SetActive(false);
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!Imperium.IsArenaLoaded.Value || Imperium.GameManager.IsGameLoading) return;

        if (timer >= 0)
        {
            timer -= Time.deltaTime;
        }
        else if (!isDone)
        {
            Deactivate();
            isDone = true;
        }
    }

    private void OnDestroy() => Destroy(sphere);
}