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
}