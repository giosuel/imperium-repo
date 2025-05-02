#region

using System;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core.Input.InputUtilsStatic;

[AttributeUsage(AttributeTargets.Property)]
public class InputActionAttribute : Attribute
{
    [Obsolete("Prefer using the named optional params instead.")]
    public InputActionAttribute(string action, string kbmPath)
    {
        ActionId = action;
        KbmPath = kbmPath;
    }

    /// <param name="kbmPath">The default bind for Keyboard and Mouse devices</param>
    public InputActionAttribute(string kbmPath)
    {
        KbmPath = kbmPath;
    }

    public readonly string KbmPath;

    /// <summary>
    ///     Overrides the generated actionId for this <see cref="InputAction" />.
    ///     <remarks>Only needs to be unique within your mod</remarks>
    /// </summary>
    public string ActionId { get; set; }

    public InputActionType ActionType { get; set; } = InputActionType.Button;

    /// <summary>
    ///     Sets the interactions of the kbm binding.
    /// </summary>
    public string KbmInteractions { get; set; }

    /// <summary>
    ///     Sets the interactions of the gamepad binding.
    /// </summary>
    public string GamepadInteractions { get; set; }

    /// <summary>
    ///     Override the display name of the keybind in-game.
    /// </summary>
    public string Name { get; set; }
}