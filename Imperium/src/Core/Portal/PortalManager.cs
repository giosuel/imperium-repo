using System;
using System.Collections.Generic;
using BepInEx.Bootstrap;
using ExitGames.Client.Photon.StructWrapping;
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

    internal static void RegisterTestPortal()
    {
        var inputFieldBinding = new ImpBinding<string>("");
        var dropdownBinding = new ImpBinding<int>(0);
        var movementSpeedBinding = new ImpBinding<float>(1f);

        Imperium.IsLevelLoaded.OnUpdate += value => Imperium.IO.LogInfo($"Imperium.IsArenaLoaded: {value}");
        API.State.IsLevelLoaded.OnUpdate += value => Imperium.IO.LogInfo($"API.State.IsArenaLoaded: {value}");

        API.Portal.ForGuid(LCMPluginInfo.PLUGIN_GUID)
            .InSection("General Config")
            .Register(
                new ImpPortalTextField("Global Modifier", inputFieldBinding, "Generated")
                    .SetTooltip(new ImpPortalTooltip("Global Modifier", "Bla bla bla"))
                    .SetInteractableBinding(API.State.IsGameLevel),
                new ImpPortalSlider("Global Speed", new ImpBinding<float>(5f), 10, 100)
            );

        API.Portal.ForGuid(LCMPluginInfo.PLUGIN_GUID)
            .InSection("Example Enemy")
            .Register(
                new ImpPortalDropdown("Behaviour", dropdownBinding, ["Passive", "Active"], "Generated", allowReset: false),
                new ImpPortalNumberField(
                    "Current Health",
                    new ImpBinding<int>(100),
                    0,
                    100
                ),
                new ImpPortalToggle("Vision Enabled", new ImpBinding<bool>()),
                new ImpPortalToggle("Hearing Enabled", new ImpBinding<bool>()),
                new ImpPortalToggle("Disabled", new ImpBinding<bool>()),
                new ImpPortalSlider("Spawn Chance", movementSpeedBinding, 0, 100, valueUnit: "%"),
                new ImpPortalButton("Spawn", () => {}),
                new ImpPortalButton("Despawn", () => {})
            );
    }
}