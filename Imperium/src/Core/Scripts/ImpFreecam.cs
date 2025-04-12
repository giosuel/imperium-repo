#region

using Imperium.Core.Lifecycle;
using Imperium.Extensions;
using Imperium.Interface.LayerSelector;
using Imperium.Util;
using Librarium.Binding;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.InputSystem;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.Core.Scripts;

public class ImpFreecam : ImpScript
{
    private Camera gameplayCamera;
    private Vector2 lookInput;
    private LayerSelector layerSelector;

    private static Rect minicamRect => new(100f / Screen.width, 1 - 100f / Screen.height - 0.4f, 0.4f, 0.4f);

    private Camera camera;
    internal readonly ImpBinaryBinding IsFreecamEnabled = new(false);

    private readonly ImpBinaryBinding IsMinicamEnabled = new(false);
    private readonly ImpBinaryBinding IsMinicamFullscreenEnabled = new(false);

    private bool firstTimeOpen = true;

    private void Awake()
    {
        gameplayCamera = PlayerAvatar.instance.localCamera;

        camera = gameObject.AddComponent<Camera>();
        camera.CopyFrom(gameplayCamera);
        camera.cullingMask = Imperium.Settings.Freecam.FreecamLayerMask.Value;
        camera.farClipPlane = 2000f;
        camera.enabled = false;

        var layerSelectorObject = Instantiate(ImpAssets.LayerSelectorObject, transform);
        layerSelector = layerSelectorObject.AddComponent<LayerSelector>();
        layerSelector.InitUI(Imperium.Interface.Theme);
        layerSelector.Bind(Imperium.Settings.Freecam.LayerSelector, Imperium.Settings.Freecam.FreecamLayerMask);

        IsFreecamEnabled.onTrue += OnFreecamEnable;
        IsFreecamEnabled.onFalse += OnFreecamDisable;

        IsMinicamEnabled.onTrue += OnMinicamEnable;
        IsMinicamEnabled.onFalse += OnMinicamDisable;

        IsMinicamFullscreenEnabled.onTrue += OnMinicamFullscreenEnable;
        IsMinicamFullscreenEnabled.onFalse += OnMinicamFullscreenDisable;

        Imperium.InputBindings.BaseMap.Freecam.performed += OnFreecamToggle;
        Imperium.InputBindings.BaseMap.Minicam.performed += OnMinicamToggle;
        Imperium.InputBindings.BaseMap.MinicamFullscreen.performed += OnMinicamFullscreenToggle;
        Imperium.InputBindings.BaseMap.Reset.performed += OnFreecamReset;
        Imperium.InputBindings.FreecamMap.LayerSelector.performed += OnToggleLayerSelector;
        Imperium.Settings.Freecam.FreecamLayerMask.onUpdate += value => camera.cullingMask = value;

        // Disable freecam whenever the scene is reloaded
        Imperium.SceneLoaded.onTrigger += IsFreecamEnabled.SetFalse;
    }

    private void OnDestroy()
    {
        Imperium.InputBindings.BaseMap.Freecam.performed -= OnFreecamToggle;
        Imperium.InputBindings.BaseMap.Minicam.performed -= OnMinicamToggle;
        Imperium.InputBindings.BaseMap.MinicamFullscreen.performed -= OnMinicamFullscreenToggle;
        Imperium.InputBindings.BaseMap.Reset.performed -= OnFreecamReset;
        Imperium.InputBindings.FreecamMap.LayerSelector.performed -= OnToggleLayerSelector;
        Imperium.InputBlocker.Unblock(this);
    }

    private void OnFreecamToggle(InputAction.CallbackContext callbackContext)
    {
        if (Imperium.Interface.IsOpen() || MenuManager.instance.IsOpen() || ChatManager.instance.IsOpen()) return;

        IsFreecamEnabled.Toggle();
    }

    private void OnMinicamToggle(InputAction.CallbackContext callbackContext)
    {
        if (Imperium.Interface.IsOpen() || MenuManager.instance.IsOpen() || ChatManager.instance.IsOpen()) return;

        IsMinicamEnabled.Toggle();
    }

    private void OnMinicamFullscreenToggle(InputAction.CallbackContext callbackContext)
    {
        if (Imperium.Interface.IsOpen() ||
            MenuManager.instance.IsOpen() ||
            ChatManager.instance.IsOpen() ||
            !IsMinicamEnabled.Value) return;

        IsMinicamFullscreenEnabled.Toggle();
    }

    private void OnMinicamEnable()
    {
        if (IsFreecamEnabled.Value) IsFreecamEnabled.SetFalse();

        PlayerManager.ToggleHUD(true);
        camera.enabled = true;
        camera.rect = minicamRect;

        IsMinicamFullscreenEnabled.SetFalse();
    }

    private void OnMinicamDisable()
    {
        // Hide UI if view is not switching from minicam to freecam
        if (!IsFreecamEnabled.Value) PlayerManager.ToggleHUD(false);

        camera.enabled = false;

        camera.rect = new Rect(0, 0, 1, 1);
    }

    private void OnMinicamFullscreenEnable() => camera.rect = new Rect(0, 0, 1, 1);
    private void OnMinicamFullscreenDisable() => camera.rect = minicamRect;

    private void OnFreecamEnable()
    {
        if (!camera) return;

        Imperium.Interface.Close();

        if (IsMinicamEnabled.Value) IsMinicamEnabled.SetFalse();

        PlayerManager.ToggleHUD(true);
        Imperium.InputBindings.FreecamMap.Enable();
        camera.enabled = true;
        enabled = true;

        if (firstTimeOpen)
        {
            firstTimeOpen = false;
            camera.transform.position = PlayerAvatar.instance.localCamera.transform.position + Vector3.up * 2;
        }

        Imperium.ActiveCamera.Set(camera);
        Imperium.InputBlocker.Block(this);
    }

    private void OnFreecamDisable()
    {
        if (!camera) return;

        layerSelector.OnUIClose();

        // Hide UI if view is not switching to minimap state
        if (!IsMinicamEnabled.Value) PlayerManager.ToggleHUD(false);

        Imperium.InputBindings.FreecamMap.Disable();
        camera.enabled = false;
        enabled = false;

        Imperium.ActiveCamera.Set(PlayerAvatar.instance.localCamera);
        Imperium.InputBlocker.Unblock(this);
    }

    private void OnFreecamReset(InputAction.CallbackContext callbackContext)
    {
        if (Imperium.Interface.IsOpen() || MenuManager.instance.IsOpen() || ChatManager.instance.IsOpen()) return;

        camera.transform.position = PlayerAvatar.instance.localCamera.transform.position + Vector3.up * 2;
        Imperium.Settings.Freecam.FreecamFieldOfView.Set(ImpConstants.DefaultFOV);
    }

    private void OnToggleLayerSelector(InputAction.CallbackContext callbackContext)
    {
        if (Imperium.Interface.IsOpen() || MenuManager.instance.IsOpen() || ChatManager.instance.IsOpen()) return;

        Imperium.Settings.Freecam.LayerSelector.Set(!layerSelector.IsOpen);
        if (layerSelector.IsOpen)
        {
            layerSelector.Close();
        }
        else
        {
            layerSelector.Open();
        }
    }

    private void Update()
    {
        // The component is only enabled when the freecam is active
        // Stop update of a quick menu an ImpUI is open with freecam 
        if (Imperium.Interface.IsOpen() || MenuManager.instance.IsOpen()) return;

        var scrollValue = Imperium.InputBindings.BaseMap.Scroll.ReadValue<float>();
        Imperium.Settings.Freecam.FreecamMovementSpeed.Set(scrollValue switch
        {
            > 0 => Mathf.Min(Imperium.Settings.Freecam.FreecamMovementSpeed.Value + 1f, 100),
            < 0 => Mathf.Max(Imperium.Settings.Freecam.FreecamMovementSpeed.Value - 1f, 1f),
            _ => Imperium.Settings.Freecam.FreecamMovementSpeed.Value
        });

        if (Imperium.InputBindings.FreecamMap.IncreaseFOV.IsPressed())
        {
            Imperium.Settings.Freecam.FreecamFieldOfView.Set(
                Mathf.Min(300, Imperium.Settings.Freecam.FreecamFieldOfView.Value + 1)
            );
        }

        if (Imperium.InputBindings.FreecamMap.DecreaseFOV.IsPressed())
        {
            Imperium.Settings.Freecam.FreecamFieldOfView.Set(
                Mathf.Max(1, Imperium.Settings.Freecam.FreecamFieldOfView.Value - 1)
            );
        }

        camera.fieldOfView = Imperium.Settings.Freecam.FreecamFieldOfView.Value;

        var cameraTransform = transform;

        var rotation = Imperium.InputBindings.BaseMap.Look.ReadValue<Vector2>();
        lookInput.x += rotation.x * 0.004f * GameplayManager.instance.aimSensitivity;
        lookInput.y += rotation.y * 0.004f * GameplayManager.instance.aimSensitivity;

        // Clamp the Y rotation to [-90;90] so the camera can't turn on it's head
        lookInput.y = Mathf.Clamp(lookInput.y, -90, 90);

        cameraTransform.rotation = Quaternion.Euler(-lookInput.y, lookInput.x, 0);

        var movement = Imperium.InputBindings.BaseMap.Movement.ReadValue<Vector2>();
        var movementY = Imperium.InputBindings.BaseMap.FlyAscend.IsPressed() ? 1 :
            Imperium.InputBindings.BaseMap.FlyDescend.IsPressed() ? -1 : 0;
        var deltaMove = new Vector3(movement.x, movementY, movement.y)
                        * (Imperium.Settings.Freecam.FreecamMovementSpeed.Value * Time.deltaTime);
        cameraTransform.Translate(deltaMove);
    }
}