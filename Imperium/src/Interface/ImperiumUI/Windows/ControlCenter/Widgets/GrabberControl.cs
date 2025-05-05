#region

using Imperium.Interface.Common;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ControlCenter.Widgets;

internal class GrabberControl : ImpWidget
{
    protected override void InitWidget()
    {
        InitGrabberSettings();
        InitPhysicsSettings();
    }

    private void InitGrabberSettings()
    {
        ImpToggle.Bind(
            "GrabberSettings/Sti" +
            "ckyGrabber",
            container: transform,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.StickyGrabber
        );

        ImpSlider.Bind(
            path: "BaseRange",
            container: transform,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.BaseRange
        );

        ImpSlider.Bind(
            path: "GrabStrength",
            container: transform,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.GrabStrength
        );

        ImpSlider.Bind(
            path: "ThrowStrength",
            container: transform,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.ThrowStrength
        );

        ImpSlider.Bind(
            path: "ReleaseDistance",
            container: transform,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.ReleaseDistance
        );

        ImpSlider.Bind(
            path: "MinDistance",
            container: transform,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.MinDistance
        );

        ImpSlider.Bind(
            path: "MaxDistance",
            container: transform,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.MaxDistance
        );
    }

    private void InitPhysicsSettings()
    {
        ImpSlider.Bind(
            path: "SpringConstant",
            container: transform,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.SpringConstant
        );

        ImpSlider.Bind(
            path: "DampingConstant",
            container: transform,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.DampingConstant
        );

        ImpSlider.Bind(
            path: "ForceConstant",
            container: transform,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.ForceConstant
        );

        ImpSlider.Bind(
            path: "ForceMax",
            container: transform,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.ForceMax
        );
    }
}