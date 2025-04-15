#region

using UnityEngine;

#endregion

namespace Imperium.Core.Scripts;

public class ImpNightVision : ImpScript
{
    private Light FarLight;
    private Light NearLight;

    private void Awake()
    {
        var mapCamera = Imperium.Player.localCamera.transform;
        transform.SetParent(PlayerAvatar.instance.localCamera.transform);

        NearLight = new GameObject("Near").AddComponent<Light>();
        NearLight.transform.SetParent(transform);
        NearLight.transform.position = mapCamera.position + Vector3.down * 80f;
        NearLight.range = 70f;
        NearLight.color = new Color(0.875f, 0.788f, 0.791f, 1);

        FarLight = new GameObject("Far").AddComponent<Light>();
        FarLight.transform.SetParent(transform);
        FarLight.transform.position = mapCamera.position + Vector3.down * 30f;
        FarLight.range = 500f;
    }

    private void Update()
    {
        // FarLight.enabled = PlayerAvatar.instance.nightVision.enabled;
        // NearLight.enabled = PlayerAvatar.instance.nightVision.enabled;

        // NearLight.intensity = Imperium.Settings.Player.NightVision.Value * 100f;
        // FarLight.intensity = Imperium.Settings.Player.NightVision.Value * 1100f;
    }
}