#region

using Imperium.Interface.Common;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ControlCenter.Widgets;

internal class PlayerSettings : ImpWidget
{
    protected override void InitWidget()
    {
        ImpToggle.Bind(
            "PlayerSettings/InfiniteEnergy",
            container: transform,
            valueBinding: Imperium.Settings.Player.InfiniteEnergy,
            theme: theme,
            parentWindow: window
        );

        ImpToggle.Bind(
            "PlayerSettings/GodMode",
            container: transform,
            valueBinding: Imperium.Settings.Player.GodMode,
            theme: theme,
            parentWindow: window
        );

        ImpToggle.Bind(
            "PlayerSettings/Invisibility",
            container: transform,
            valueBinding: Imperium.Settings.Player.Invisibility,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Invisibility",
                Description = "Makes the local player invisible to entities.",
                Tooltip = tooltip
            },
            parentWindow: window
        );

        ImpToggle.Bind(
            "PlayerSettings/Muted",
            container: transform,
            valueBinding: Imperium.Settings.Player.Muted,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Muted",
                Description = "Surpresses movement noises made by the local player.",
                Tooltip = tooltip
            },
            parentWindow: window
        );

        ImpToggle.Bind(
            "PlayerSettings/NoTumbleMode",
            container: transform,
            valueBinding: Imperium.Settings.Player.NoTumbleMode,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "No Tumble Mode",
                Description = "Disables tumbling when getting hit.",
                Tooltip = tooltip
            },
            parentWindow: window
        );

        ImpToggle.Bind(
            "PlayerSettings/EnableFlying",
            container: transform,
            valueBinding: Imperium.Settings.Player.EnableFlying,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "WIP",
                Description = "Flying is not implemented yet.",
                Tooltip = tooltip
            },
            parentWindow: window
        );

        ImpSlider.Bind(
            path: "NightVision",
            container: transform,
            minValue: 0,
            maxValue: 100,
            valueBinding: Imperium.Settings.Player.NightVision,
            valueUnit: "%",
            theme: theme,
            parentWindow: window
        );

        ImpSlider.Bind(
            path: "FieldOfView",
            container: transform,
            valueBinding: Imperium.Settings.Player.CustomFieldOfView,
            minValue: 50,
            maxValue: 160,
            valueUnit: "°",
            useWholeNumbers: true,
            theme: theme,
            parentWindow: window
        );

        ImpSlider.Bind(
            path: "MovementSpeed",
            container: transform,
            valueBinding: Imperium.Settings.Player.MovementSpeed,
            minValue: 0,
            maxValue: 10,
            theme: theme,
            parentWindow: window
        );

        ImpSlider.Bind(
            path: "JumpForce",
            container: transform,
            valueBinding: Imperium.Settings.Player.JumpForce,
            minValue: 0,
            maxValue: 100,
            theme: theme,
            parentWindow: window
        );

        // ImpSlider.Bind(
        //     path: "Right/FlyingSpeed",
        //     container: content,
        //     theme: theme,
        //     valueBinding: Imperium.Settings.Player.FlyingSpeed
        // );
    }
}