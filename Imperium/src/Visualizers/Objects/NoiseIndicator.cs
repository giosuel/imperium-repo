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

    private Vector3 worldPosition;
    private bool isDone;
    private TMP_Text indicatorDistanceText;
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
        indicatorImage = transform.Find("Image").GetComponent<Image>();
        indicatorTransform = GetComponent<RectTransform>();
        indicatorArrow = transform.Find("Arrow").GetComponent<RectTransform>();

        sphere = ImpGeometry.CreatePrimitive(
            PrimitiveType.Sphere,
            objectParent,
            ImpAssets.FresnelGreen
        );

        // By default, spheres are disabled. Only the map camera enables them during its render step.
        sphere.SetActive(false);
    }

    internal void Activate(Vector3 position, float radius, int time)
    {
        timer = time;
        totalTime = time;
        isDone = false;

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
        isDone = true;
        sphere.SetActive(false);
        gameObject.SetActive(false);
    }

    internal void ToggleSphere(bool isShown) => sphere.SetActive(isShown);

    private void LateUpdate()
    {
        if (timer >= 0)
        {
            var camera = Imperium.ActiveCamera.Value;

            var targetPos = camera.WorldToScreenPoint(worldPosition);
            if (targetPos.z < 0) targetPos *= -1;

            var activeTexture = camera.activeTexture;
            var scaleFactor = activeTexture.width / canvasRect.sizeDelta.x;

            var textureX = activeTexture.width / scaleFactor;
            var textureY = activeTexture.height / scaleFactor;

            var margin = activeTexture.width / scaleFactor * 0.05f;
            var positionX = targetPos.x / scaleFactor;
            var positionY = targetPos.y / scaleFactor;
            var clamped = false;
            if (positionX < margin || positionX > textureX - margin)
            {
                positionX = Math.Clamp(positionX, margin, textureX - margin);
                clamped = true;
            }

            if (positionY < margin || positionY > textureY - margin * 2)
            {
                positionY = Math.Clamp(positionY, margin, textureY - margin * 2);
                clamped = true;
            }

            indicatorArrow.gameObject.SetActive(clamped);
            indicatorTransform.anchoredPosition = new Vector2(positionX, positionY);
            var playerPosition = Imperium.Player.transform.position;
            transform.localScale = Vector3.one * Math.Clamp(
                5 / Vector3.Distance(playerPosition, worldPosition),
                0.5f, 1f
            );

            var angle = Vector2.SignedAngle(
                new Vector2(0, 1),
                new Vector2(textureX / 2f - positionX, textureY / 2f - positionY).normalized
            );
            indicatorArrow.rotation = Quaternion.Euler(0, 0, angle + 180);

            timer -= Time.deltaTime;
            indicatorDistanceText.text = $"{Vector3.Distance(playerPosition, worldPosition):0.0}m";

            indicatorDistanceText.color = ImpUtils.Interface.ChangeAlpha(
                indicatorColor, indicatorImage.color.a - Time.deltaTime / totalTime
            );
            indicatorImage.color = ImpUtils.Interface.ChangeAlpha(
                indicatorColor, indicatorImage.color.a - Time.deltaTime / totalTime
            );
        }
        else if (!isDone)
        {
            Deactivate();
        }
    }

    private void OnDestroy()
    {
        Destroy(sphere);
    }
}