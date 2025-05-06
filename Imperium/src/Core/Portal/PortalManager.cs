using System.Collections.Generic;
using Imperium.API.Types.Portals;
using Librarium.Binding;

namespace Imperium.Core.Portal;

internal class PortalManager
{
    private readonly Dictionary<string, ImpPortal> registeredPortals = new();

    internal ImpPortal GetPortalFor(string guid)
    {
        if (!registeredPortals.TryGetValue(guid, out var portal))
        {
            portal = new ImpPortal();
            registeredPortals[guid] = portal;
        }

        return portal;
    }

    private void test()
    {
        var inputFieldBinding = new ImpBinding<string>();
        var dropdownBinding = new ImpBinding<int>();
        var dropdownOptions = new List<string>(["option1", "option2"]);

        ImpPortal.ForGuid("test")
            .InSection("section")
            .Register(
                new ImpPortalButton("Test Button", ButtonCallback)
                    .SetTooltip("Test Button", "does something")
                    .SetInteractableBinding(new ImpBinaryBinding(SemiFunc.IsMultiplayer()))
            )
            .Register(
                new ImpPortalTextField("Test Button", inputFieldBinding, 0, 10)
                    .SetTooltip("Some field", "something something")
                    .SetInteractableBinding(new ImpBinaryBinding(SemiFunc.IsMultiplayer()))
            )
            .Register(
                new ImpPortalDropdown("Test dropdown", dropdownBinding, dropdownOptions)
                    .SetTooltip("Some dropdown", "something something")
                    .SetInteractableBinding(new ImpBinaryBinding(SemiFunc.IsMultiplayer()))
            );
        return;

        void ButtonCallback()
        {
            /* something */
        }
    }
}