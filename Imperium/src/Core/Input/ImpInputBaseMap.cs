#region

using Imperium.Core.Input.InputUtilsStatic;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core.Input;

internal sealed class ImpInputBaseMap : LcInputActions
{
    [InputAction("", Name = "Movement")] internal InputAction Movement { get; set; }

    [InputAction("<Pointer>/delta", Name = "Look")]
    internal InputAction Look { get; set; }

    [InputAction("<Mouse>/scroll/y", Name = "Scroll")]
    internal InputAction Scroll { get; set; }

    [InputAction("<Keyboard>/o", Name = "Tape Measure")]
    internal InputAction TapeMeasure { get; set; }

    [InputAction("<Keyboard>/g", Name = "Teleport")]
    internal InputAction Teleport { get; set; }

    [InputAction("<Keyboard>/f", Name = "Freecam")]
    internal InputAction Freecam { get; set; }

    [InputAction("<Keyboard>/m", Name = "Minimap")]
    internal InputAction Minimap { get; set; }

    [InputAction("<Keyboard>/x", Name = "Minicam")]
    internal InputAction Minicam { get; set; }

    [InputAction("<Keyboard>/f11", Name = "Fullscreen Minicam")]
    internal InputAction MinicamFullscreen { get; set; }

    [InputAction("<Keyboard>/z", Name = "Toggle HUD")]
    internal InputAction ToggleHUD { get; set; }

    [InputAction("<Keyboard>/r", Name = "Reset")]
    internal InputAction Reset { get; set; }

    [InputAction("<Keyboard>/space", Name = "Toggle Flight")]
    internal InputAction ToggleFlight { get; set; }

    [InputAction("<Keyboard>/space", Name = "Flying Ascend", KbmInteractions = "hold(duration=0.1)")]
    internal InputAction FlyAscend { get; set; }

    [InputAction("<Keyboard>/ctrl", Name = "Flying Descend", KbmInteractions = "hold(duration=0.1)")]
    internal InputAction FlyDescend { get; set; }

    [InputAction("<Mouse>/leftButton", Name = "Map Rotate")]
    internal InputAction MapRotate { get; set; }

    [InputAction("<Mouse>/rightButton", Name = "Map Pan")]
    internal InputAction MapPan { get; set; }

    [InputAction("<Keyboard>/upArrow", Name = "Previous Item")]
    internal InputAction PreviousItem { get; set; }

    [InputAction("<Keyboard>/downArrow", Name = "Next Item")]
    internal InputAction NextItem { get; set; }

    [InputAction("<Keyboard>/return", Name = "Select Item")]
    internal InputAction SelectItem { get; set; }

    [InputAction("<Mouse>/middleButton", Name = "Teleport to Waypoint")]
    internal InputAction TeleportWaypoint { get; set; }

    [InputAction("<Keyboard>/delete", Name = "Delete Waypoint")]
    internal InputAction DeleteWaypoint { get; set; }

    internal ImpInputBaseMap()
    {
        // ReSharper disable once ExpressionIsAlwaysNull
        // This property will always be set by the base constructor via reflection
        var movementComposite = Movement.AddCompositeBinding("2DVector");
        movementComposite.With("Up", "<Keyboard>/w");
        movementComposite.With("Down", "<Keyboard>/s");
        movementComposite.With("Left", "<Keyboard>/a");
        movementComposite.With("Right", "<Keyboard>/d");
    }
}