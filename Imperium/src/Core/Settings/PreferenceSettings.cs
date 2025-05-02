#region

using BepInEx.Configuration;
using Librarium;
using Librarium.Binding;

#endregion

namespace Imperium.Core.Settings;

internal class PreferenceSettings(ConfigFile config, IBinding<bool> isSceneLoaded, IBinding<bool> isEnabled)
    : SettingsContainer(config)
{
    internal readonly ImpConfig<bool> EnableImperium = new(
        config,
        "Preferences.General",
        "EnableImperium",
        true
    );

    internal readonly ImpConfig<bool> GeneralLogging = new(config, "Preferences.General", "GeneralLogging", true);
    internal readonly ImpConfig<bool> LeftHandedMode = new(config, "Preferences.General", "LeftHandedMode", false);
    internal readonly ImpConfig<bool> CustomWelcome = new(config, "Preferences.General", "CustomWelcome", true);
    internal readonly ImpConfig<bool> ShowTooltips = new(config, "Preferences.General", "Tooltips", true);
    internal readonly ImpConfig<bool> PlaySounds = new(config, "Preferences.General", "Sounds", true);

    internal readonly ImpConfig<string> ImperiumWindowLayout = new(
        config,
        "Preferences.Appearance",
        "WindowLayout",
        ""
    );

    internal readonly ImpConfig<string> Theme = new(
        config,
        "Preferences.Appearance",
        "Theme",
        "Repo"
    );

    internal readonly ImpConfig<bool> AllowClients = new(
        config,
        "Preferences.Host",
        "AllowClients",
        true,
        ignoreRefresh: true
    );

    internal readonly ImpConfig<bool> UnityExplorerMouseFix = new(
        config,
        "Preferences.General",
        "UnityExplorerMouseFix",
        true
    );

    internal readonly ImpConfig<bool> NotificationsGodMode = new(
        config,
        "Preferences.Notifications",
        "GodMode",
        true
    );

    internal readonly ImpConfig<bool> NotificationsOracle = new(
        config,
        "Preferences.Notifications",
        "Oracle",
        true
    );

    internal readonly ImpConfig<bool> NotificationsSpawnReports = new(
        config,
        "Preferences.Notifications",
        "SpawnReports",
        true
    );

    internal readonly ImpConfig<bool> NotificationsConfirmation = new(
        config,
        "Preferences.Notifications",
        "Confirmation",
        true
    );

    internal readonly ImpConfig<bool> NotificationsEntities = new(
        config,
        "Preferences.Notifications",
        "Entities",
        true
    );

    internal readonly ImpConfig<bool> NotificationsSpawning = new(
        config,
        "Preferences.Notifications",
        "Spawning",
        true
    );

    internal readonly ImpConfig<bool> NotificationsAccessControl = new(
        config,
        "Preferences.Notifications",
        "AccessControl",
        true
    );

    internal readonly ImpConfig<bool> NotificationsServer = new(
        config,
        "Preferences.Notifications",
        "Server",
        true
    );

    internal readonly ImpConfig<bool> NotificationsOther = new(
        config,
        "Preferences.Notifications",
        "Other",
        true
    );

    internal readonly ImpConfig<bool> QuickloadAutoLaunch = new(
        config,
        "Preferences.Quickload",
        "AutoLaunch",
        false
    );

    internal readonly ImpConfig<LaunchMode> QuickloadLaunchMode = new(
        config,
        "Preferences.Quickload",
        "LaunchMode",
        LaunchMode.Singleplayer
    );
}