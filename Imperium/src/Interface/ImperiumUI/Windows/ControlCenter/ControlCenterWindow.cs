#region

using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.ControlCenter.Widgets;
using Imperium.Types;
using Imperium.Util;
using Librarium;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ControlCenter;

internal class ControlCenterWindow : ImperiumWindow
{
    private Transform content;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        InitPlayerSettings();

        RegisterWidget<GrabberControl>(content, "Left");

        if (Random.Range(0, 100) >= 99) titleBox.Find("Title").GetComponent<TMP_Text>().text = "Emporium Control Panel";
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("Separator", Variant.DARKER)
        );
    }

    private void InitPlayerSettings()
    {
        ImpToggle.Bind(
            "Right/PlayerSettings/InfiniteEnergy",
            content,
            Imperium.Settings.Player.InfiniteEnergy,
            theme: theme
        );

        ImpToggle.Bind(
            "Right/PlayerSettings/GodMode",
            content,
            Imperium.Settings.Player.GodMode,
            theme: theme
        );

        ImpToggle.Bind(
            "Right/PlayerSettings/Invisibility",
            content,
            Imperium.Settings.Player.Invisibility,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Invisibility",
                Description = "Makes the local player invisible to entities.",
                Tooltip = tooltip
            }
        );

        ImpToggle.Bind(
            "Right/PlayerSettings/Muted",
            content,
            Imperium.Settings.Player.Muted,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Muted",
                Description = "Surpresses movement noises made by the local player.",
                Tooltip = tooltip
            }
        );

        ImpToggle.Bind(
            "Right/PlayerSettings/NoTumbleMode",
            content,
            Imperium.Settings.Player.NoTumbleMode,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "No Tumble Mode",
                Description = "Disables tumbling when hit",
                Tooltip = tooltip
            }
        );

        ImpToggle.Bind(
            "Right/PlayerSettings/EnableFlying",
            content,
            Imperium.Settings.Player.EnableFlying,
            theme: theme
        );

        ImpToggle.Bind(
            "Right/PlayerSettings/DevMode",
            content,
            Imperium.Settings.Player.DevMode,
            theme: theme
        );

        ImpToggle.Bind(
            "Right/PlayerSettings/SlowMode",
            content,
            Imperium.Settings.Player.SlowMode,
            theme: theme
        );

        ImpSlider.Bind(
            path: "Right/NightVision",
            container: content,
            minValue: 0,
            maxValue: 100,
            valueBinding: Imperium.Settings.Player.NightVision,
            valueUnit: "%",
            theme: theme
        );

        ImpSlider.Bind(
            path: "Right/FieldOfView",
            container: content,
            valueBinding: Imperium.Settings.Player.CustomFieldOfView,
            minValue: 50,
            maxValue: 160,
            valueUnit: "°",
            useWholeNumbers: true,
            theme: theme
        );

        ImpSlider.Bind(
            path: "Right/MovementSpeed",
            container: content,
            valueBinding: Imperium.Settings.Player.MovementSpeed,
            minValue: 0,
            maxValue: 10,
            theme: theme
        );

        ImpSlider.Bind(
            path: "Right/JumpForce",
            container: content,
            valueBinding: Imperium.Settings.Player.JumpForce,
            minValue: 0,
            maxValue: 100,
            theme: theme
        );

        // ImpSlider.Bind(
        //     path: "Right/FlyingSpeed",
        //     container: content,
        //     theme: theme,
        //     valueBinding: Imperium.Settings.Player.FlyingSpeed
        // );
    }

    protected override void OnOpen()
    {
        Imperium.Settings.Player.SlowMode.Set(PlayerController.instance.debugSlow);

        Imperium.GameManager.TotalHaul.Refresh();
        Imperium.GameManager.GroupCurrency.Refresh();
    }
}