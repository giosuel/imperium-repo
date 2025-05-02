#region

using System.Collections.Generic;

#endregion

namespace Imperium.Core;

internal class InputBlocker
{
    private readonly HashSet<object> inputBlockers = [];

    internal void Block(object blocker)
    {
        if (inputBlockers.Count == 0) DisableActions();
        inputBlockers.Add(blocker);
    }

    internal void Unblock(object blocker)
    {
        inputBlockers.Remove(blocker);
        if (inputBlockers.Count == 0) EnableActions();
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