#region

using Imperium.Core.Input.InputUtilsStatic;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Core.Input;

public class ImpInputInterfaceMap : LcInputActions
{
    [InputAction("<Keyboard>/F1", Name = "Imperium UI")]
    internal InputAction ImperiumUI { get; set; }

    [InputAction("<Keyboard>/F2", Name = "Spawning UI")]
    internal InputAction SpawningUI { get; set; }

    [InputAction("<Keyboard>/F8", Name = "Map UI")]
    internal InputAction MapUI { get; set; }

    [InputAction("<Keyboard>/F4", Name = "Console UI")]
    internal InputAction ConsoleUI { get; set; }

    [InputAction("<Keyboard>/F3", Name = "Teleportation Window")]
    internal InputAction TeleportWindow { get; set; }
}