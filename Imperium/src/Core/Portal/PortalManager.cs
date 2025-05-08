using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using Imperium.API.Types.Portals;
using Librarium.Binding;

namespace Imperium.Core.Portal;

internal class PortalManager
{
    // A special pre-defined portal for transient runtime elements
    internal readonly ImpPortal RuntimePortal = new("Runtime");

    internal readonly Dictionary<string, ImpPortal> RegisteredPortals = new();

    internal ImpPortal GetPortalFor(string guid)
    {
        if (!RegisteredPortals.TryGetValue(guid, out var portal))
        {
            if (!Chainloader.PluginInfos.TryGetValue(guid, out var pluginInfo))
            {
                throw new ArgumentOutOfRangeException($"Unable to find mod with GUID {guid}");
            }

            portal = new ImpPortal(pluginInfo.Metadata.Name);
            RegisteredPortals[guid] = portal;
        }

        return portal;
    }

    internal ImpPortal GetRuntimePortal() => RuntimePortal;

    internal static void RegisterTestPortal()
    {
        var inputFieldBinding = new ImpBinding<string>();
        var dropdownBinding = new ImpBinding<int>();
        var dropdownOptions = new List<string>(["option1", "option2"]);

        API.Portal.ForGuid(LCMPluginInfo.PLUGIN_GUID)
            .InSection("section")
            .Register(
                new ImpPortalButton("Test Button", ButtonCallback)
                    .SetTooltip(new ImpPortalTooltip("Test button", "does something"))
                    .SetInteractableBinding(new ImpBinaryBinding(true))
            )
            .Register(
                new ImpPortalTextField(
                        "Test text field",
                        inputFieldBinding,
                        "Generated"
                    )
                    .SetTooltip(new ImpPortalTooltip("Test input field", "does something"))
                    .SetInteractableBinding(new ImpBinaryBinding(true))
            )
            .Register(
                new ImpPortalDropdown(
                        "Test dropdown",
                        dropdownBinding,
                        dropdownOptions,
                        "Nothing"
                    )
                    .SetTooltip(new ImpPortalTooltip("Test dropdown", "does something"))
                    .SetInteractableBinding(new ImpBinaryBinding(true))
            );
        return;

        void ButtonCallback()
        {
            Imperium.IO.LogInfo("Someone pressed le funni button");
        }
    }
}