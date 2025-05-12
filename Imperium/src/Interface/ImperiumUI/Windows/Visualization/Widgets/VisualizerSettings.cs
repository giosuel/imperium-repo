#region

using Imperium.Interface.Common;
using Librarium;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Visualization.Widgets;

internal class VisualizerSettings : ImpWidget
{
    protected override void InitWidget()
    {
        ImpToggle.Bind(
            "General/SmoothAnimations",
            transform,
            Imperium.Settings.Visualization.SmoothAnimations,
            theme: theme
        );
        ImpToggle.Bind(
            "SSOverlays/AlwaysOnTop",
            transform,
            Imperium.Settings.Visualization.SSAlwaysOnTop,
            theme: theme
        );
        ImpToggle.Bind(
            "SSOverlays/AutoScale",
            transform,
            Imperium.Settings.Visualization.SSAutoScale,
            theme: theme
        );
        ImpToggle.Bind(
            "SSOverlays/HideDespawned",
            transform,
            Imperium.Settings.Visualization.SSHideDespawned,
            theme: theme
        );

        ImpSlider.Bind(
            path: "OverlayScale",
            container: transform,
            valueBinding: Imperium.Settings.Visualization.SSOverlayScale,
            minValue: 0.1f,
            maxValue: 3f,
            valueUnit: "x",
            handleFormatter: Formatting.FormatFloatToThreeDigits,
            theme: theme,
            interactableInvert: true,
            interactableBindings: Imperium.Settings.Visualization.SSAutoScale
        );

        ImpButton.Bind(
            "ClearObjects",
            transform,
            () => Imperium.Visualization.ClearObjects(),
            theme: theme
        );
    }
}