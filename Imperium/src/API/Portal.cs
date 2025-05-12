#region

using Imperium.API.Types.Portals;

#endregion

namespace Imperium.API;

public static class Portal
{
    /// <summary>
    ///     Returns an existing or creates a new portal for a given mod GUID.
    /// </summary>
    /// <param name="guid">The GUID of the calling mod</param>
    /// <returns></returns>
    public static ImpPortal ForGuid(string guid) => Imperium.PortalManager.GetPortalFor(guid);

    /// <summary>
    ///     Returns a portal that can be used for registering portal elements at runtime with mods like Unity Explorer.
    /// </summary>
    /// <returns></returns>
    public static ImpPortal ForRuntime() => Imperium.PortalManager.RuntimePortal;
}