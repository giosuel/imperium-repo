#region

using System;
using Imperium.Core.Lifecycle;
using Imperium.Interface.MapUI;
using Imperium.Util;
using Librarium.Binding;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using Object = UnityEngine.Object;

#endregion

namespace Imperium.Core.Scripts;

public class ImpMapCamera : ImpScript
{
    private Color oldColor;
    private float oldFogDistance;
    private bool isAvatarVisible;

    private Camera camera;

    private void Awake()
    {
        camera = gameObject.AddComponent<Camera>();
        camera.enabled = false;
        camera.orthographic = true;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = Color.black;
    }

    private void OnPreRender()
    {
        if (Camera.current != camera) return;

        oldColor = RenderSettings.ambientLight;
        oldFogDistance = RenderSettings.fogEndDistance;
        RenderSettings.ambientLight = new Color(1, 1, 1);
        RenderSettings.fogEndDistance = 500f;

        isAvatarVisible = PlayerAvatar.instance.playerAvatarVisuals.meshParent.activeSelf;
        if (!isAvatarVisible) PlayerManager.ToggleLocalAvatar(true);
    }


    private void OnPostRender()
    {
        if (Camera.current != camera) return;

        RenderSettings.ambientLight = oldColor;
        RenderSettings.fogEndDistance = oldFogDistance;

        // Disable avatar again if it was enabled in pre-render
        if (!isAvatarVisible) PlayerManager.ToggleLocalAvatar(false);
    }
}