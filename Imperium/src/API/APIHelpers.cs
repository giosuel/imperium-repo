namespace Imperium.API;

internal static class APIHelpers
{
    /// <summary>
    ///     Throws an <see cref="ImperiumAPIException" /> when Imperium is not ready to serve API calls.
    /// </summary>
    internal static void AssertImperiumReady()
    {
        if (Imperium.IsImperiumLaunched) return;

        throw new ImperiumAPIException("Failed to execute API call. Imperium has not yet been initialized.");
    }

    /// <summary>
    ///     Throws an <see cref="ImperiumAPIException" /> when the game arena is current not loaded.
    /// </summary>
    internal static void AssertArenaLoaded()
    {
        if (Imperium.IsArenaLoaded) return;

        throw new ImperiumAPIException("The game arena has not yet been loaded.");
    }
}