using System.Collections.Generic;

namespace Imperium.Core;

internal class InputBlocker
{
    private readonly HashSet<object> inputBlockers = [];

    internal void Block(object blocker)
    {
        if (inputBlockers.Count == 0) DisableActions();
        inputBlockers.Add(blocker);

        Imperium.IO.LogInfo($"Block - Input blockers: {inputBlockers.Count}");
    }

    internal void Unblock(object blocker)
    {
        inputBlockers.Remove(blocker);
        if (inputBlockers.Count == 0) EnableActions();

        Imperium.IO.LogInfo($"Unblock - Input blockers: {inputBlockers.Count}");
    }

    private static void DisableActions()
    {
        foreach (var action in InputManager.instance.inputActions) action.Value.Disable();
    }

    private static void EnableActions()
    {
        foreach (var action in InputManager.instance.inputActions) action.Value.Enable();
    }
}