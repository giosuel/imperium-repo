#region

using System.Collections.Generic;
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

    public const int DefaultFOV = 66;
    public const float DefaultGrabStrength = 1f;
    public const float DefaultThrowStrength = 0f;
    public const float DefaultMovementSpeed = 2f;
    public const float DefaultSlideTime = 0.5f;
    public const float DefaultJumpForce = 17f;

    internal const float DefaultMapCameraScale = 20f;

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

    internal const string GeneralSaveFile = "LCGeneralSaveData";

    internal static readonly LayerMask IndicatorMask = LayerMask.GetMask("Default", "PhysGrabObjectHinge");

    internal static readonly LayerMask TapeIndicatorMask = LayerMask.GetMask("Default", "PhysGrabObjectHinge");

    /*
     * Maps the class names of default insights to more recognizable names.
     */
    internal static readonly Dictionary<string, string> ClassNameMap = new()
    {
        { nameof(EnemyParent), "Enemies" },
        { nameof(ExtractionPoint), "Extraction Points" },
    };

    // Items that have no spawn prefab
    public static readonly HashSet<string> ItemBlacklist = [];
}