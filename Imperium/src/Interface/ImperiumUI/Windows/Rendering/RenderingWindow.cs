#region

using Imperium.Interface.Common;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Rendering;

internal class RenderingWindow : ImperiumWindow
{
    protected override void InitWindow()
    {
        var content = transform.Find("Content");

        // ImpSlider.Bind(
        //     path: "FogStart",
        //     container: content,
        //     valueBinding: Imperium.Settings.Rendering.FogStart,
        //     indicatorFormatter: value => $"{Formatting.FormatFloatToThreeDigits(value)}",
        //     theme: theme,
        //     defaultValueOverrideFunc: () => LevelGenerator.Instance.Level.FogStartDistance,
        //     negativeIsDefault: true
        // );
        //
        // ImpSlider.Bind(
        //     path: "FogEnd",
        //     container: content,
        //     valueBinding: Imperium.Settings.Rendering.FogEnd,
        //     indicatorFormatter: value => $"{Formatting.FormatFloatToThreeDigits(value)}",
        //     theme: theme,
        //     defaultValueOverrideFunc: () => LevelGenerator.Instance.Level.FogEndDistance,
        //     negativeIsDefault: true
        // );

        ImpToggle.Bind("PostProcessing/Main", content, Imperium.Settings.Rendering.PostMain, theme: theme);
        ImpToggle.Bind("PostProcessing/Darkness", content, Imperium.Settings.Rendering.PostDarkness, theme: theme);
        ImpToggle.Bind("PostProcessing/Overlay", content, Imperium.Settings.Rendering.PostOverlay, theme: theme);

        ImpToggle.Bind("PlayerAvatar/MainCamera", content, Imperium.Settings.Rendering.AvatarInMain, theme: theme);
        ImpToggle.Bind("PlayerAvatar/Freecam", content, Imperium.Settings.Rendering.AvatarInFreecam, theme: theme);
    }
}