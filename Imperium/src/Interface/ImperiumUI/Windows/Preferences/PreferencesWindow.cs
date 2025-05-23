#region

using System;
using System.Linq;
using BepInEx.Bootstrap;
using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Librarium.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Preferences;

internal class PreferencesWindow : ImperiumWindow
{
    private Transform content;
    private TMP_Dropdown launchModeDropdown;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        var general = content.Find("Grid/General/General");
        ImpToggle.Bind("LoggingToggle", general, Imperium.Settings.Preferences.GeneralLogging, theme: theme);
        ImpToggle.Bind("LeftHandedToggle", general, Imperium.Settings.Preferences.LeftHandedMode, theme: theme);
        ImpToggle.Bind("TooltipsToggle", general, Imperium.Settings.Preferences.ShowTooltips, theme: theme);

        // Play click sounds needs to be the opposite of the setting here, as we are about to toggle it
        ImpToggle.Bind(
            "SoundsToggle",
            general,
            Imperium.Settings.Preferences.PlaySounds,
            theme: theme,
            playClickSound:
            !Imperium.Settings.Preferences.PlaySounds.Value
        );

        ImpToggle.Bind(
            "UEMouseFixToggle",
            general,
            Imperium.Settings.Preferences.UnityExplorerMouseFix,
            theme: theme,
            interactableBindings: new ImpBinding<bool>(Chainloader.PluginInfos.ContainsKey("com.sinai.unityexplorer")),
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Unity Explorer Mouse Fix",
                Description = "Makes it so the camera doesn't follow the mouse\nwhen unity explorer is open.",
                Tooltip = tooltip
            }
        );

        ImpToggle.Bind(
            "SkipLoading",
            general,
            Imperium.Settings.Preferences.SkipLoading,
            theme: theme
            // tooltipDefinition: new TooltipDefinition
            // {
            //     Title = " Explorer Mouse Fix",
            //     Description = "Makes it so the camera doesn't follow the mouse\nwhen unity explorer is open.",
            //     Tooltip = tooltip
            // }
        );

        var hosting = content.Find("Grid/Hosting/Hosting");
        ImpToggle.Bind(
            "AllowClients",
            hosting,
            Imperium.Settings.Preferences.AllowClients,
            theme: theme,
            interactableBindings: new ImpBinding<bool>(SemiFunc.IsMasterClientOrSingleplayer()),
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Allow Imperium Clients",
                Description = "Whether clients are allowed to use Imperium in the current lobby.",
                Tooltip = tooltip
            }
        );

        var notifications = content.Find("Grid/Notifications/Notifications");
        ImpToggle.Bind(
            "GodModeToggle",
            notifications,
            Imperium.Settings.Preferences.NotificationsGodMode,
            theme: theme
        );

        ImpToggle.Bind(
            "OracleToggle",
            notifications,
            Imperium.Settings.Preferences.NotificationsOracle,
            theme: theme
        );

        ImpToggle.Bind(
            "SpawnReportsToggle",
            notifications,
            Imperium.Settings.Preferences.NotificationsSpawnReports,
            theme: theme
        );

        ImpToggle.Bind(
            "ConfirmationToggle",
            notifications,
            Imperium.Settings.Preferences.NotificationsConfirmation,
            theme: theme
        );

        ImpToggle.Bind(
            "EntitiesToggle",
            notifications,
            Imperium.Settings.Preferences.NotificationsEntities,
            theme: theme
        );

        ImpToggle.Bind(
            "SpawningToggle",
            notifications,
            Imperium.Settings.Preferences.NotificationsSpawning,
            theme: theme
        );

        ImpToggle.Bind(
            "AccessControl",
            notifications,
            Imperium.Settings.Preferences.NotificationsAccessControl,
            theme: theme
        );

        ImpToggle.Bind(
            "OtherToggle",
            notifications,
            Imperium.Settings.Preferences.NotificationsOther,
            theme: theme
        );


        InitQuickload();
        InitThemes();

        ImpButton.Bind("Buttons/FactoryReset", content, Imperium.Settings.FactoryReset, theme: theme);
        ImpButton.Bind("Buttons/ResetUI", content, Imperium.Interface.ResetUI, theme: theme);
    }

    private void InitQuickload()
    {
        var quickloadToggles = content.Find("Grid/Quickload/QuickloadToggles");
        ImpToggle.Bind(
            "AutoLaunchToggle",
            quickloadToggles,
            Imperium.Settings.Preferences.QuickloadAutoLaunch,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Quickload Auto Launch",
                Description = "Whether to auto launch into a mode on start-up.",
                Tooltip = tooltip
            }
        );

        var quickloadBottom = content.Find("Grid/Quickload/QuickloadBottom");
        launchModeDropdown = quickloadBottom.Find("LaunchMode/Dropdown").GetComponent<TMP_Dropdown>();
        launchModeDropdown.options = Enum.GetValues(typeof(LaunchMode))
            .Cast<LaunchMode>()
            .OrderBy(mode => mode)
            .Select(mode => new TMP_Dropdown.OptionData(mode.ToString()))
            .ToList();

        launchModeDropdown.onValueChanged.AddListener(value =>
        {
            Imperium.Settings.Preferences.QuickloadLaunchMode.Set((LaunchMode)value);
        });
        launchModeDropdown.value = (int)Imperium.Settings.Preferences.QuickloadLaunchMode.Value;

        ImpUtils.Interface.BindDropdownInteractable(
            Imperium.Settings.Preferences.QuickloadAutoLaunch,
            quickloadBottom.Find("LaunchMode")
        );
    }

    private void InitThemes()
    {
        var themeContainer = content.Find("Appearance");
        var hoveredTheme = new ImpBinding<string>();

        for (var i = 0; i < themeContainer.childCount; i++)
        {
            var themePanel = themeContainer.GetChild(i);

            ImpMultiSelectEntry.Bind(
                themePanel.name,
                themeContainer.GetChild(i).gameObject,
                Imperium.Settings.Preferences.Theme,
                hoveredTheme
            );
        }
    }

    protected override void OnThemeUpdate(ImpTheme updatedTheme)
    {
        ImpThemeManager.Style(
            updatedTheme,
            content,
            new StyleOverride("Appearance", Variant.DARKER)
        );

        // Launch mode dropdown
        ImpThemeManager.Style(
            updatedTheme,
            launchModeDropdown.transform,
            new StyleOverride("", Variant.FOREGROUND),
            new StyleOverride("Arrow", Variant.FOREGROUND),
            new StyleOverride("Template", Variant.DARKER),
            new StyleOverride("Template/Viewport/Content/Item/Background", Variant.DARKER),
            new StyleOverride("Template/Scrollbar", Variant.DARKEST),
            new StyleOverride("Template/Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );

        // Theme entries
        var themeContainer = content.Find("Appearance");
        for (var i = 0; i < themeContainer.childCount; i++)
        {
            ImpThemeManager.Style(
                updatedTheme,
                themeContainer.GetChild(i),
                new StyleOverride("Selected", Variant.FADED),
                new StyleOverride("Hover", Variant.DARKER)
            );
        }
    }
}