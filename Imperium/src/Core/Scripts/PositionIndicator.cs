#region

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using ImpMath = Imperium.Util.ImpMath;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.Core.Scripts;

public class PositionIndicator : ImpScript
{
    private Transform origin;
    private GameObject indicator;
    private LineRenderer arcLine;
    private bool castToGround;

    private Action<Vector3> registeredCallback;

    internal bool IsActive { get; private set; }

    private void Awake()
    {
        indicator = transform.Find("Indicator").gameObject;
        arcLine = indicator.GetComponent<LineRenderer>();

        Deactivate();

        // Deactivate position indicator whenever the scene is reloaded
        Imperium.IsArenaLoaded.onTrigger += Deactivate;
    }

    internal void Activate(Action<Vector3> callback, Transform originTransform = null, bool castGround = true)
    {
        castToGround = castGround;
        registeredCallback = callback;

        origin = originTransform ?? Imperium.ActiveCamera.Value.transform;

        IsActive = true;
        indicator.SetActive(true);

        Imperium.Interface.Close();

        // Disable conflicting vanilla binds
        InputManager.instance.inputActions[InputKey.Menu].Disable();
        InputManager.instance.inputActions[InputKey.Grab].Disable();
        InputManager.instance.inputActions[InputKey.Rotate].Disable();

        Imperium.InputBindings.StaticMap["Escape"].performed += OnExitAction;
        Imperium.InputBindings.StaticMap["LeftClick"].performed += OnLeftClick;
        Imperium.InputBindings.StaticMap["RightClick"].performed += OnExitAction;

        Imperium.InputBindings.BaseMap.TapeMeasure.Disable();
    }

    internal void Deactivate()
    {
        IsActive = false;
        indicator.SetActive(false);
        registeredCallback = null;

        // Re-enable conflicting vanilla actions
        InputManager.instance.inputActions[InputKey.Menu].Enable();
        InputManager.instance.inputActions[InputKey.Grab].Enable();
        InputManager.instance.inputActions[InputKey.Rotate].Enable();

        Imperium.InputBindings.StaticMap["Escape"].performed -= OnExitAction;
        Imperium.InputBindings.StaticMap["LeftClick"].performed -= OnLeftClick;
        Imperium.InputBindings.StaticMap["RightClick"].performed -= OnExitAction;

        Imperium.InputBindings.BaseMap.TapeMeasure.Enable();
    }

    private void OnExitAction(InputAction.CallbackContext context) => Deactivate();

    private void OnLeftClick(InputAction.CallbackContext context)
    {
        if (IsActive) SubmitIndicator();
    }

    private void SubmitIndicator()
    {
        registeredCallback?.Invoke(indicator.transform.position);
        Deactivate();
    }

    private void Update()
    {
        if (!IsActive || !origin) return;

        var forward = origin.forward;
        var ray = new Ray(origin.position, forward);

        // Raycast to player look position
        Physics.Raycast(ray, out var hitInfo, 1000, ImpConstants.IndicatorMask);

        var endPosition = hitInfo.point;

        if (castToGround)
        {
            // Raycast from look position to the ground below
            Physics.Raycast(
                new Ray(hitInfo.point, Vector3.down),
                out var groundInfo, 100, ImpConstants.IndicatorMask
            );

            endPosition = hitInfo.normal.y > 0 ? hitInfo.point : groundInfo.point;
        }

        indicator.transform.position = endPosition;
        arcLine.transform.RotateAround(endPosition, Vector3.up, 80 * Time.deltaTime);

        arcLine.positionCount = 100;
        arcLine.startWidth = 0.08f;
        arcLine.endWidth = 0.08f;

        if (endPosition == Vector3.zero)
        {
            arcLine.gameObject.SetActive(false);
            return;
        }

        arcLine.gameObject.SetActive(true);

        var offset = Imperium.Settings.Preferences.LeftHandedMode.Value ? -origin.right : origin.right;
        var arcStartPosition = origin.position + offset;

        for (var i = 0; i < 100; i++)
        {
            var position2D = ImpMath.SampleQuadraticBezier(
                arcStartPosition.y,
                endPosition.y,
                arcStartPosition.y + Math.Clamp(Math.Abs(forward.y * 20), 0, 10),
                i / 100f
            );
            arcLine.SetPosition(i, new Vector3(
                Mathf.Lerp(arcStartPosition.x, endPosition.x, i / 100f),
                position2D,
                Mathf.Lerp(arcStartPosition.z, endPosition.z, i / 100f))
            );
        }
    }

    private void OnDestroy() => Imperium.InputBlocker.Unblock(this);
}