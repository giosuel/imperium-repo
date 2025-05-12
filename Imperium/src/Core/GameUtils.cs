#region

using UnityEngine;

#endregion

namespace Imperium.Core;

public static class GameUtils
{
    internal static void PlayClip(AudioClip audioClip, bool randomize = false)
    {
        if (!Imperium.Settings.Preferences.PlaySounds.Value) return;
        // RoundManager.PlayRandomClip(Imperium.HUDManager.UIAudio, [audioClip], randomize);
    }
}