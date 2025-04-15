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

    private RenderTexture cameraTexture;
    internal Light CameraLight;

    private void Awake()
    {
        var originalMapCam = Map.Instance.transform.parent.Find("Active/Player/Dirt Finder Map Camera").GetComponent<Camera>();

        // var cameraMapObject = Instantiate(originalMapCam, transform);
        var cameraMapObject = new GameObject("Imp_MapCamera");
        cameraMapObject.transform.SetParent(transform);
        cameraMapObject.transform.position = PlayerAvatar.instance.transform.position + Vector3.up * 20f;
        cameraMapObject.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // Camera top-down light
        CameraLight = new GameObject("Imp_MapLight").AddComponent<Light>();
        CameraLight.transform.SetParent(cameraMapObject.transform);
        CameraLight.transform.position = originalMapCam.transform.position + Vector3.up * 30f;
        CameraLight.type = LightType.Directional;
        CameraLight.intensity = 100f;
        CameraLight.enabled = false;

        var mapCamera = cameraMapObject.AddComponent<ImpMapCamera>();
        Camera = mapCamera.GetComponent<Camera>();

        Camera.cullingMask = Imperium.Settings.Map.CameraLayerMask.Value;
        Imperium.Settings.Map.CameraLayerMask.onUpdate += value => Camera.cullingMask = value;

        Camera.orthographicSize = Imperium.Settings.Map.CameraZoom.Value;
        Imperium.Settings.Map.CameraZoom.onUpdate += value => Camera.orthographicSize = value;

        // We need to make sure that the far clip isn't smaller than the near clip and vice versa
        Camera.farClipPlane = Mathf.Max(CameraFarClip.Value, CameraNearClip.Value);
        CameraFarClip.onUpdate += value => Camera.farClipPlane = Mathf.Max(CameraNearClip.Value + 1, value);

        Camera.nearClipPlane = Mathf.Min(CameraFarClip.Value, CameraNearClip.Value);
        CameraNearClip.onUpdate += value => Camera.nearClipPlane = Mathf.Min(CameraFarClip.Value - 1, value);

        Imperium.InputBindings.BaseMap.Scroll.performed += OnMouseScroll;

        Minimap = Instantiate(ImpAssets.MinimapOverlayObject, transform).AddComponent<MinimapOverlay>();
        Minimap.InitUI(Imperium.Interface.Theme);
    }

    internal void SetCameraPosition(Vector3 position, Vector3 target)
    {
        Camera.transform.position = position;
        Camera.transform.LookAt(target);
    }

    internal RenderTexture GetRenderTexture(int width, int height)
    {
        if (!cameraTexture || cameraTexture.width != width || cameraTexture.height != height)
        {
            cameraTexture = new RenderTexture(width, height, 16);
            cameraTexture.Create();

            Camera.targetTexture = cameraTexture;
        }

        return cameraTexture;
    }

    private void OnMouseScroll(InputAction.CallbackContext context)
    {
        if (!Imperium.Interface.IsOpen<MapUI>() && !Imperium.InputBindings.StaticMap["Alt"].IsPressed()) return;

        var multiplier = Mathf.Sign(context.ReadValue<float>()) * (Imperium.Settings.Map.CameraZoom.Value / 10);
        Imperium.Settings.Map.CameraZoom.Set(
                Mathf.Clamp(Imperium.Settings.Map.CameraZoom.Value - multiplier, 1, 100)
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

    private void Update()
    {
        if (!Imperium.IsArenaLoaded.Value) return;

        Camera.transform.position = Imperium.ActiveCamera.Value.transform.position + Vector3.up * 20f;
    }
}