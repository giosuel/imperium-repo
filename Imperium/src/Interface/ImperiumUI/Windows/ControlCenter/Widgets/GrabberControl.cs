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
            "GrabberSettings/StickyGrabber",
            container: transform,
            valueBinding: Imperium.Settings.Grabber.StickyGrabber,
            theme: theme
        );

        ImpSlider.Bind(
            path: "BaseRange",
            container: transform,
            valueBinding: Imperium.Settings.Grabber.BaseRange,
            minValue: 0,
            maxValue: 20,
            theme: theme
        );

        ImpSlider.Bind(
            path: "GrabStrength",
            container: transform,
            valueBinding: Imperium.Settings.Grabber.GrabStrength,
            minValue: 0,
            maxValue: 20,
            theme: theme
        );

        ImpSlider.Bind(
            path: "ThrowStrength",
            container: transform,
            valueBinding: Imperium.Settings.Grabber.ThrowStrength,

            minValue: 0,
            maxValue: 20,
            theme: theme
        );

        ImpSlider.Bind(
            path: "ReleaseDistance",
            container: transform,
            valueBinding: Imperium.Settings.Grabber.ReleaseDistance,
            minValue: 0,
            maxValue: 20,
            theme: theme
        );

        ImpSlider.Bind(
            path: "MinDistance",
            container: transform,
            valueBinding: Imperium.Settings.Grabber.MinDistance,
            minValue: 0,
            maxValue: 10,
            theme: theme
        );

        ImpSlider.Bind(
            path: "MaxDistance",
            container: transform,
            valueBinding: Imperium.Settings.Grabber.MaxDistance,
            minValue: 0,
            maxValue: 10,
            theme: theme
        );
    }

    private void InitPhysicsSettings()
    {
        ImpSlider.Bind(
            path: "SpringConstant",
            container: transform,
            valueBinding: Imperium.Settings.Grabber.SpringConstant,
            minValue: 0,
            maxValue: 5,
            theme: theme
        );

        ImpSlider.Bind(
            path: "DampingConstant",
            container: transform,
            valueBinding: Imperium.Settings.Grabber.DampingConstant,
            minValue: 0,
            maxValue: 5,
            theme: theme
        );

        ImpSlider.Bind(
            path: "ForceConstant",
            container: transform,
            valueBinding: Imperium.Settings.Grabber.ForceConstant,
            minValue: 0,
            maxValue: 20,
            theme: theme
        );

        ImpSlider.Bind(
            path: "ForceMax",
            container: transform,
            valueBinding: Imperium.Settings.Grabber.ForceMax,
            minValue: 0,
            maxValue: 20,
            theme: theme
        );
    }
}