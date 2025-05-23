#region

using UnityEngine;

#endregion

namespace Imperium.Core.Scripts;

public class ImpNightVision : ImpScript
{
    private void LateUpdate()
    {
        if (!Imperium.IsGameLevel.Value) return;

        if (Imperium.Settings.Player.NightVision.Value > 0)
        {
            var intensity = Imperium.Settings.Player.NightVision.Value / 100 * 2;
            RenderSettings.ambientLight = new Color(intensity, intensity, intensity);
        }
    }
}