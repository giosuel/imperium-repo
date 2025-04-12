#region

using System;
using Imperium.Interface.MapUI;
using Imperium.Util;
using Librarium.Binding;
using UnityEngine;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core.Scripts;

public class ImpMap : ImpScript
{
    internal Camera Camera { get; private set; }
    internal MinimapOverlay Minimap { get; private set; }

    internal readonly ImpBinding<float> CameraNearClip = new(ImpConstants.DefaultMapCameraNearClip);
    internal readonly ImpBinding<float> CameraFarClip = new(ImpConstants.DefaultMapCameraFarClip);

    private void Awake()
    {
        var originalMapCam = Imperium.Player.localCamera;

        var cameraMapObject = new GameObject("Imp_MapCamera");
        cameraMapObject.transform.SetParent(transform);
        cameraMapObject.transform.position = originalMapCam.transform.position + Vector3.up * 20f;
        cameraMapObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Camera top-down light
        var light = new GameObject("Imp_MapLight").AddComponent<Light>();
        light.transform.SetParent(cameraMapObject.transform);
        light.transform.position = originalMapCam.transform.position + Vector3.up * 30f;
        light.range = 200f;
        // light.intensity = 1000f;
        light.intensity = 0;
        light.gameObject.layer = LayerMask.NameToLayer("HelmetVisor");

        Camera = cameraMapObject.AddComponent<Camera>();
        Camera.enabled = false;
        Camera.orthographic = true;

        Camera.cullingMask = Imperium.Settings.Map.CameraLayerMask.Value;
        Imperium.Settings.Map.CameraLayerMask.onUpdate += value => Camera.cullingMask = value;

        Camera.orthographicSize = Imperium.Settings.Map.CameraZoom.Value;
        Imperium.Settings.Map.CameraZoom.onUpdate += value => Camera.orthographicSize = value;

        // We need to make sure that the far clip isn't smaller than the near clip and vice versa
        Camera.farClipPlane = Mathf.Max(CameraFarClip.Value, CameraNearClip.Value);
        CameraFarClip.onUpdate += value => Camera.farClipPlane = Mathf.Max(CameraNearClip.Value + 1, value);

        Camera.nearClipPlane = Mathf.Min(CameraFarClip.Value, CameraNearClip.Value);
        CameraNearClip.onUpdate += value => Camera.nearClipPlane = Mathf.Min(CameraFarClip.Value - 1, value);

        // var hdCameraData = cameraMapObject.AddComponent<HDAdditionalCameraData>();
        // hdCameraData.customRenderingSettings = true;
        // hdCameraData.renderingPathCustomFrameSettings.SetEnabled(FrameSettingsField.Volumetrics, false);
        // hdCameraData.renderingPathCustomFrameSettingsOverrideMask.mask[(int)FrameSettingsField.Volumetrics] = true;

        // Imperium.IngamePlayerSettings.playerInput.actions.FindAction("SwitchItem").performed += OnMouseScroll;

        Minimap = Instantiate(ImpAssets.MinimapOverlayObject).AddComponent<MinimapOverlay>();
        Minimap.InitUI(Imperium.Interface.Theme);
        Minimap.onOpen += OnMinimapOpen;
        Minimap.onClose += OnMinimapClose;
    }

    private static void OnMouseScroll(InputAction.CallbackContext context)
    {
        if (!Imperium.Interface.IsOpen<MapUI>() && !Imperium.InputBindings.StaticMap["Alt"].IsPressed()) return;

        var multiplier = Imperium.Settings.Map.CameraZoom.Value / 100 * 8;
        Imperium.Settings.Map.CameraZoom.Set(
            Mathf.Clamp(
                Imperium.Settings.Map.CameraZoom.Value - context.ReadValue<float>() * multiplier,
                1,
                100
            )
        );
    }

    internal void SetCameraClipped(bool isClipped)
    {
        CameraNearClip.Set(
            isClipped ? ImpConstants.DefaultMapCameraNearClip : ImpConstants.DefaultMapCameraNearClipFreeLook
        );
        CameraFarClip.Set(
            isClipped ? ImpConstants.DefaultMapCameraFarClip : ImpConstants.DefaultMapCameraFarClipFreeLook
        );
    }

    private void OnMinimapOpen(bool wasOpen)
    {
        Imperium.Map.Camera.enabled = true;
        Imperium.Map.Camera.rect = Minimap.CameraRect;
    }

    private void OnMinimapClose()
    {
        // Hide the map only when neither the minimap not the map are open
        if (!Imperium.Interface.IsOpen<MapUI>()) Camera.enabled = false;
    }

    private void Update()
    {
        Camera.transform.position = Imperium.Player.localCamera.transform.position + Vector3.up * 20f;
    }
}