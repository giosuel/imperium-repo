namespace Imperium.API;

internal static class APIHelpers
{
    /// <summary>
    ///     Throws an <see cref="ImperiumAPIException" /> when Imperium is not ready to serve API calls.
    /// </summary>
    internal static void AssertImperiumReady()
    {
        if (Imperium.IsImperiumEnabled.Value) return;

        throw new ImperiumAPIException("Failed to execute API call. Imperium has not yet been enabled.");
    }
}