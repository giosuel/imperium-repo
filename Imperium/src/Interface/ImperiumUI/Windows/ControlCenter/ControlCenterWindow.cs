#region

using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
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

        InitQuotaAndCredits();
        InitPlayerSettings();
        InitGameSettings();

        if (Random.Range(0, 100) >= 99) titleBox.Find("Title").GetComponent<TMP_Text>().text = "Emporium Control Panel";
    }

    protected override void OnThemePrimaryUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("Separator", Variant.DARKER)
        );
    }

    private void InitQuotaAndCredits()
    {
        ImpInput.Bind(
            "Left/GroupCurrency/Input",
            content,
            Imperium.GameManager.GroupCurrency,
            min: 0,
            theme: theme
        );

        ImpInput.Bind(
            "Left/TotalHaul/Input",
            content,
            Imperium.GameManager.TotalHaul,
            min: 0,
            theme: theme
        );

        ImpToggle.Bind(
            "Left/LowHaul",
            content,
            Imperium.GameManager.LowHaul,
            theme
        );

        ImpButton.Bind(
            "Left/ShopButtons/BuyItems",
            content,
            () =>
            {
                StatsManager.instance.BuyAllItems();
                SemiFunc.StatSyncAll();
            },
            theme
        );
    }

    private void InitGameSettings()
    {
    }

    private void InitPlayerSettings()
    {
        ImpToggle.Bind(
            "Right/PlayerSettings/InfiniteEnergy",
            content,
            Imperium.Settings.Player.InfiniteEnergy,
            theme
        );

        ImpToggle.Bind(
            "Right/PlayerSettings/GodMode",
            content,
            Imperium.Settings.Player.GodMode,
            theme
        );

        ImpToggle.Bind(
            "Right/PlayerSettings/Invisibility",
            content,
            Imperium.Settings.Player.Invisibility, theme,
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
            theme,
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
            theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "No Tumble Mode",
                Description = "Disables tumbling :(",
                Tooltip = tooltip
            }
        );

        ImpToggle.Bind(
            "Right/PlayerSettings/EnableFlying",
            content,
            Imperium.Settings.Player.EnableFlying,
            theme
        );

        ImpToggle.Bind(
            "Right/PlayerSettings/DevMode",
            content,
            Imperium.Settings.Player.DevMode,
            theme
        );

        ImpToggle.Bind(
            "Right/PlayerSettings/SlowMode",
            content,
            Imperium.Settings.Player.SlowMode,
            theme
        );

        ImpSlider.Bind(
            path: "Right/FieldOfView",
            container: content,
            valueBinding: Imperium.Settings.Player.CustomFieldOfView,
            defaultValueOverride: ImpConstants.DefaultFOV,
            indicatorUnit: "°",
            theme: theme
        );

        ImpSlider.Bind(
            path: "Right/MovementSpeed",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Player.MovementSpeed
        );

        ImpSlider.Bind(
            path: "Right/SlideTime",
            container: content,
            theme: theme,
            indicatorFormatter: value => $"{Formatting.FormatFloatToThreeDigits(value)}",
            valueBinding: Imperium.Settings.Player.SlideTime
        );

        ImpSlider.Bind(
            path: "Right/JumpForce",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Player.JumpForce
        );

        ImpSlider.Bind(
            path: "Right/FlyingSpeed",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Player.FlyingSpeed
        );

        ImpSlider.Bind(
            path: "Right/NightVision",
            container: content,
            theme: theme,
            valueBinding: Imperium.Settings.Player.NightVision,
            indicatorUnit: "%"
        );
    }

    protected override void OnOpen()
    {
        Imperium.Settings.Player.SlowMode.Set(PlayerController.instance.debugSlow);

        Imperium.GameManager.TotalHaul.Refresh();
        Imperium.GameManager.GroupCurrency.Refresh();
    }
}