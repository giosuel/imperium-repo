#region

using System;
using Imperium.Extensions;
using Imperium.Util;
using Librarium;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core.Scripts;

public class TapeMeasure : ImpScript
{
    internal bool IsActive { get; private set; }

    private GameObject indicator;
    private RectTransform canvasRect;
    private RectTransform panelRect;
    private TMP_Text distanceText;

    private bool hasAborted;

    private Vector3? startPosition;
    private Vector3? endPosition;
    private Vector3 currentLookPosition;
    private Vector3 currentLookNormal;

    private GameObject startMarker;
    private GameObject endMarker;

    private LineRenderer tapeLine;

    private float rotationZ;

    private LayerMask tapeLayerMask;

    private readonly Vector3[] Axes =
    [
        Vector3.up, Vector3.down, Vector3.left, Vector3.right, Vector3.forward, Vector3.back
    ];

    private void Awake()
    {
        indicator = transform.Find("Indicator").gameObject;
        canvasRect = transform.Find("Canvas").GetComponent<RectTransform>();
        panelRect = transform.Find("Canvas/Panel").GetComponent<RectTransform>();
        distanceText = transform.Find("Canvas/Panel/Number").GetComponent<TMP_Text>();

        canvasRect.GetComponent<Canvas>().sortingOrder = 2;

        Imperium.InputBindings.BaseMap.TapeMeasure.performed += OnTapeOpen;

        tapeLine = Geometry.CreateLine(
            transform,
            useWorldSpace: true,
            startColor: new Color(1, 1, 1),
            endColor: new Color(1, 1, 1),
            thickness: 0.03f
        );
        startMarker = Geometry.CreatePrimitive(PrimitiveType.Sphere, transform, color: new Color(1, 1, 1), 0.1f);
        endMarker = Geometry.CreatePrimitive(PrimitiveType.Sphere, transform, color: new Color(1, 1, 1), 0.1f);

        Imperium.Freecam.IsFreecamEnabled.onUpdate += OnFreecamToggle;
        OnFreecamToggle(Imperium.Freecam.IsFreecamEnabled.Value);
        OnExitAction();

        // Deactivate tape measure whenever the scene is reloaded
        Imperium.IsArenaLoaded.onTrigger += Deactivate;
    }

    private void Activate()
    {
        IsActive = true;
        hasAborted = false;
        indicator.SetActive(true);

        startPosition = null;
        endPosition = null;

        startMarker.SetActive(false);
        endMarker.SetActive(false);
        panelRect.gameObject.SetActive(false);

        // Disable conflicting vanilla binds
        InputManager.instance.inputActions[InputKey.Menu].Disable();
        InputManager.instance.inputActions[InputKey.Grab].Disable();
        InputManager.instance.inputActions[InputKey.Rotate].Disable();

        Imperium.InputBindings.StaticMap["Escape"].performed += OnExitAction;
        Imperium.InputBindings.StaticMap["LeftClick"].performed += OnLeftClick;
        Imperium.InputBindings.StaticMap["RightClick"].performed += OnExitAction;

        Imperium.InputBindings.BaseMap.Teleport.Disable();
    }

    internal void Deactivate()
    {
        IsActive = false;
        indicator.SetActive(false);

        // Re-enable conflicting vanilla actions
        InputManager.instance.inputActions[InputKey.Menu].Enable();
        InputManager.instance.inputActions[InputKey.Grab].Enable();
        InputManager.instance.inputActions[InputKey.Rotate].Enable();

        Imperium.InputBindings.StaticMap["Escape"].performed -= OnExitAction;
        Imperium.InputBindings.StaticMap["LeftClick"].performed -= OnLeftClick;
        Imperium.InputBindings.StaticMap["RightClick"].performed -= OnExitAction;

        Imperium.InputBindings.BaseMap.Teleport.Enable();
    }

    private void OnExitAction(InputAction.CallbackContext context = default)
    {
        // Preemptive exit, so disable currently active objects
        startMarker.SetActive(false);
        endMarker.SetActive(false);
        panelRect.gameObject.SetActive(false);
        tapeLine.gameObject.SetActive(false);
        hasAborted = true;

        Deactivate();
    }

    private void OnLeftClick(InputAction.CallbackContext context = default)
    {
        if (!IsActive) return;

        if (!startPosition.HasValue)
        {
            startPosition = currentLookPosition;
            startMarker.transform.position = startPosition.Value;
            startMarker.SetActive(true);
        }
        else if (!endPosition.HasValue)
        {
            endPosition = currentLookPosition;
            endMarker.transform.position = endPosition.Value;
            endMarker.SetActive(true);
            Deactivate();
        }
    }

    private void OnTapeOpen(InputAction.CallbackContext callbackContext)
    {
        if (IsActive)
        {
            OnExitAction();
        }
        else if (!Imperium.Interface.IsOpen() && !MenuManager.instance.IsOpen() && !ChatManager.instance.IsOpen())
        {
            Activate();
        }
    }

    private void OnFreecamToggle(bool isOn)
    {
        tapeLayerMask = isOn ? Imperium.Settings.Freecam.FreecamLayerMask.Value : ImpConstants.TapeIndicatorMask;
    }

    private void LateUpdate()
    {
        if (!Imperium.ActiveCamera.Value) return;
        var originCamera = Imperium.ActiveCamera.Value;

        // Indicator rotation animation values
        if (rotationZ > 360)
        {
            rotationZ = 0;
        }
        else
        {
            rotationZ += 100 * Time.deltaTime;
        }

        // Raycast to current player look position
        var ray = new Ray(
            originCamera.transform.position + originCamera.transform.forward * 0.4f,
            originCamera.transform.forward
        );
        Physics.Raycast(ray, out var hitInfo, 1000, tapeLayerMask);

        currentLookPosition = hitInfo.point;
        currentLookNormal = hitInfo.normal;

        if (startPosition.HasValue)
        {
            var tapeLineVector = (endPosition ?? hitInfo.point) - startPosition.Value;

            // Axis snapping when first position is selected and the Alt key is pressed
            if (Imperium.InputBindings.StaticMap["Alt"].IsPressed())
            {
                Vector3 snappingAxis = default;
                var minAngle = 360f;
                foreach (var axis in Axes)
                {
                    var axisAngle = Vector3.Angle(tapeLineVector, axis);
                    if (axisAngle < minAngle)
                    {
                        snappingAxis = axis;
                        minAngle = axisAngle;
                    }
                }

                // The furthest possible point on the axis is defined by the position of the first object in the axis' ray
                var endPointRay = new Ray(startPosition.Value, snappingAxis);
                Physics.Raycast(endPointRay, out var endPointHit, 1000, tapeLayerMask);

                var player = PlayerAvatar.instance.localCamera.transform;

                // Angle between the player forward and the snapping axis
                var angleToAxis = Vector3.Angle(player.forward, snappingAxis);

                // Calculate the required length of the player forward with sine
                var oppositeSide = Vector3.Distance(originCamera.transform.position, startPosition.Value);
                var lookDistance = oppositeSide / Mathf.Sin(Mathf.Deg2Rad * angleToAxis);

                // Project the forward vector onto the snapping axis
                var forwardToStart = startPosition.Value - player.position;
                var extendedForward = player.forward * lookDistance;
                var lookDifference = extendedForward - forwardToStart;
                var projection = Vector3.ClampMagnitude(
                    Vector3.Project(lookDifference, snappingAxis),
                    endPointHit.distance
                );

                /*
                 * If the projection vector was clamped to the end point, align the indicator with the end point
                 *  and snap the indicator to the end point. Otherwise, align the indicator with the opposite of
                 *  the player's forward to make it face the player.
                 */
                if (Vector3.Distance(endPointHit.point, startPosition.Value + projection) < 0.5f)
                {
                    currentLookNormal = endPointHit.normal;
                    currentLookPosition = endPointHit.point;
                }
                else
                {
                    currentLookNormal = -player.forward;
                    currentLookPosition = startPosition.Value + projection;
                }

                tapeLineVector = currentLookPosition - startPosition.Value;
            }

            var panelWorldPosition = startPosition.Value + tapeLineVector / 2;
            var screenPosition = originCamera.WorldToScreenPoint(panelWorldPosition);

            var activeCameraTexture = originCamera.targetTexture;

            var scaleFactorX = activeCameraTexture.width / canvasRect.sizeDelta.x;
            var scaleFactorY = activeCameraTexture.height / canvasRect.sizeDelta.y;

            var positionX = screenPosition.x / scaleFactorX;
            var positionY = screenPosition.y / scaleFactorY;
            panelRect.anchoredPosition = new Vector2(positionX, positionY);

            var distanceToTape = (originCamera.transform.position - panelWorldPosition).magnitude;
            panelRect.localScale = Vector3.one * Mathf.Clamp(4 / distanceToTape, 0.5f, 2);
            panelRect.gameObject.SetActive(!hasAborted && screenPosition.z > 0);

            if (IsActive)
            {
                tapeLine.gameObject.SetActive(true);
                distanceText.text = $"{tapeLineVector.magnitude:0.00}u";

                Geometry.SetLinePositions(
                    tapeLine,
                    startPosition.Value,
                    endPosition ?? currentLookPosition
                );
            }
        }
        else
        {
            tapeLine.gameObject.SetActive(false);
        }

        // Position the indicator
        indicator.transform.position = currentLookPosition;
        indicator.transform.LookAt(currentLookPosition + currentLookNormal);
        indicator.transform.RotateAround(currentLookPosition, currentLookNormal, rotationZ);
    }

    private void OnDestroy() => Imperium.InputBlocker.Unblock(this);
}