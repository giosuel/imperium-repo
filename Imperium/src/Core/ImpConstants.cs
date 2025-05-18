#region

using System;
using System.Collections.Generic;
using Imperium.Interface.ImperiumUI.Windows.ArenaControl;
using Imperium.Interface.ImperiumUI.Windows.ControlCenter;
using Imperium.Interface.ImperiumUI.Windows.EventLog;
using Imperium.Interface.ImperiumUI.Windows.LevelGeneration;
using Imperium.Interface.ImperiumUI.Windows.ObjectExplorer;
using Imperium.Interface.ImperiumUI.Windows.Portal;
using Imperium.Interface.ImperiumUI.Windows.Preferences;
using Imperium.Interface.ImperiumUI.Windows.Rendering;
using Imperium.Interface.ImperiumUI.Windows.Teleport;
using Imperium.Interface.ImperiumUI.Windows.Upgrades;
using Imperium.Interface.ImperiumUI.Windows.Visualization;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Core;

public struct ImpConstants
{
    internal struct Opacity
    {
        internal const float Enabled = 1;
        internal const float TextDisabled = 0.2f;
        internal const float ImageDisabled = 0.5f;
    }

    public const int DefaultFOV = 70;
    public const float DefaultGrabStrength = 1f;
    public const float DefaultThrowStrength = 0f;
    public const float DefaultMovementSpeed = 2f;
    public const float DefaultSlideTime = 0.5f;
    public const float DefaultJumpForce = 17f;

    internal const float DefaultMapCameraScale = 12f;

    internal const int DefaultMapCameraFarClip = 50;
    internal const int DefaultMapCameraNearClip = 9;

    internal const int DefaultMapCameraFarClipFreeLook = 200;
    internal const int DefaultMapCameraNearClipFreeLook = 1;

    internal const float DefaultGrabberRange = 4f;
    internal const float DefaultGrabberReleaseDistance = 8f;
    internal const float DefaultGrabberMinDistance = 1f;
    internal const float DefaultGrabberMaxDistance = 2.5f;
    internal const float DefaultGrabberSpringConstant = 0.9f;
    internal const float DefaultGrabberDampingConstant = 0.5f;
    internal const float DefaultGrabberForceConstant = 4f;
    internal const float DefaultGrabberMaxForce = 4f;

    internal const string ImperiumSaveFile = "REPO_SAVE_IMPERIUM";
    internal const string GeneralSaveFile = "LCGeneralSaveData";

    internal static readonly LayerMask IndicatorMask = LayerMask.GetMask("Default", "PhysGrabObjectHinge");

    internal static readonly LayerMask TapeIndicatorMask = LayerMask.GetMask("Default", "PhysGrabObjectHinge");

    public static readonly HashSet<string> TrueStrings = ["y", "yes", "on", "t", "true", "e", "enable"];
    public static readonly HashSet<string> FalseStrings = ["n", "no", "off", "f", "false", "d", "disable"];
    public static readonly HashSet<string> ResetStrings = ["r", "reset", "default", "def"];

    public const string GreyedOutColor = "#A6A6A6";

    internal static readonly Dictionary<Type, Sprite> WindowIconMap = new()
    {
        { typeof(ControlCenterWindow), ImpAssets.IconControlCenter },
        { typeof(EventLogWindow), ImpAssets.IconEventLog },
        { typeof(GameControlWindow), ImpAssets.IconGameControl },
        { typeof(LevelGeneration), ImpAssets.IconLevelGeneration },
        { typeof(ObjectExplorerWindow), ImpAssets.IconObjectExplorer },
        { typeof(PortalWindow), ImpAssets.IconPortal },
        { typeof(PreferencesWindow), ImpAssets.IconPreferences },
        { typeof(RenderingWindow), ImpAssets.IconRendering },
        { typeof(TeleportWindow), ImpAssets.IconTeleportation },
        { typeof(UpgradesWindow), ImpAssets.IconUpgrades },
        { typeof(VisualizationWindow), ImpAssets.IconVisualizers }
    };

    /*
     * Maps the class names of default insights to more recognizable names.
     */
    internal static readonly Dictionary<string, string> ClassNameMap = new()
    {
        { nameof(EnemyParent), "Enemies" },
        { nameof(ExtractionPoint), "Extraction Points" },
        { nameof(ValuableObject), "Valuables" },
    };

    /*
     * Levels that can't be picked in the level override.
     */
    internal static readonly List<string> LevelBlacklist =
    [
        "Lobby Menu",
        "Main Menu",
        "Recording",
        "Tutorial"
    ];

    internal static readonly Dictionary<string, string> PlayerUpgradeNameMap = new()
    {
        { "playerUpgradeHealth", "Health" },
        { "playerUpgradeStamina", "Stamina" },
        { "playerUpgradeExtraJump", "Jump" },
        { "playerUpgradeLaunch", "Launch" },
        { "playerUpgradeMapPlayerCount", "Map Count" },
        { "playerUpgradeSpeed", "Speed" },
        { "playerUpgradeStrength", "Strength" },
        { "playerUpgradeRange", "Range" },
        { "playerUpgradeThrow", "Throw" },
    };
}