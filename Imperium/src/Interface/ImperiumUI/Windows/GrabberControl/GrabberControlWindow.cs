#region

using Imperium.Interface.Common;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.GrabberControl;

internal class GrabberControlWindow : ImperiumWindow
{
    private Transform content;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        InitGrabberSettings();
        InitPhysicsSettings();
    }

    private void InitGrabberSettings()
    {
        ImpToggle.Bind(
            "GrabberSettings/StickyGrabber",
            content,
            Imperium.Settings.Grabber.StickyGrabber,
            theme
        );

        ImpSlider.Bind(
            path: "BaseRange",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.BaseRange
        );

        ImpSlider.Bind(
            path: "GrabStrength",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.GrabStrength
        );

        ImpSlider.Bind(
            path: "ThrowStrength",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.ThrowStrength
        );

        ImpSlider.Bind(
            path: "ReleaseDistance",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.ReleaseDistance
        );

        ImpSlider.Bind(
            path: "MinDistance",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.MinDistance
        );

        ImpSlider.Bind(
            path: "MaxDistance",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.MaxDistance
        );

        ImpSlider.Bind(
            path: "SpringConstant",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.SpringConstant
        );

        ImpSlider.Bind(
            path: "DampingConstant",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.DampingConstant
        );

        ImpSlider.Bind(
            path: "ForceConstant",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.ForceConstant
        );

        ImpSlider.Bind(
            path: "ForceMax",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Grabber.ForceMax
        );
    }

    private void InitPhysicsSettings()
    {
    }
}