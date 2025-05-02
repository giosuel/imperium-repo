#region

using Imperium.Core.Input.InputUtilsStatic;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core.Input;

internal sealed class ImpInputFreecamMap : LcInputActions
{
    [InputAction("<Keyboard>/v", Name = "Toggle Freehand Mode")]
    internal InputAction FreehandMode { get; set; }

    [InputAction("<Keyboard>/l", Name = "Toggle Layer Selector")]
    internal InputAction LayerSelector { get; set; }

    [InputAction("<Keyboard>/enter", Name = "Toggle Layer")]
    internal InputAction ToggleLayer { get; set; }

    [InputAction("<Keyboard>/upArrow", Name = "Previous Layer")]
    internal InputAction PreviousLayer { get; set; }

    [InputAction("<Keyboard>/downArrow", Name = "Next Layer")]
    internal InputAction NextLayer { get; set; }

    [InputAction("<Keyboard>/leftArrow", Name = "Freecam Increase FOV")]
    internal InputAction IncreaseFOV { get; set; }

    [InputAction("<Keyboard>/rightArrow", Name = "Freecam Decrease FOV")]
    internal InputAction DecreaseFOV { get; set; }
}