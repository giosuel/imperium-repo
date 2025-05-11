using Librarium.Binding;

namespace Imperium.API;

public static class State
{
    /// <summary>
    ///     Set to true when Imperium is launched and ready to serve API calls.
    /// </summary>
    public static bool IsImperiumLaunched => Imperium.IsImperiumLaunched;

    /// <summary>
    ///     Binding that indicates whether Imperium is launched and imperium access is currently granted.
    /// </summary>
    public static IBinding<bool> IsImperiumEnabled => ImpImmutableBinding<bool>.Wrap(Imperium.IsImperiumEnabled);

    /// <summary>
    ///     Binding that indicates whether the game has finished loading the current level.
    ///
    ///     Triggers after every call to <see cref="LoadingUI.StartLoading"/> and <see cref="LoadingUI.StopLoading"/>.
    /// </summary>
    public static IBinding<bool> IsLevelLoaded => ImpImmutableBinding<bool>.Wrap(Imperium.IsLevelLoaded);

    /// <summary>
    ///     Binding that indicates whether the currently loaded level is a game level and not the main menu or lobby.
    ///
    ///     Whether a level is a game level or not is decided by <see cref="Core.Lifecycle.GameManager.IsGameLevel"/>
    ///     This binding is updated every time <see cref="IsLevelLoaded"/> is udpated.
    /// </summary>
    public static IBinding<bool> IsGameLevel => ImpImmutableBinding<bool>.Wrap(Imperium.IsGameLevel);
}