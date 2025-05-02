#region

using BepInEx.Configuration;
using Imperium.Core.Settings;
using Librarium.Binding;

#endregion

namespace Imperium.Core;

/// <summary>
///     Contains all the bindings of the persistent settings of Imperium.
/// </summary>
public class ImpSettings(ConfigFile config, IBinding<bool> isArenaLoaded, IBinding<bool> isEnabled)
{
    // Indication if settings are currently being loaded (to skip notifications and other things during loading)
    internal bool IsLoading { get; private set; }

    internal readonly PlayerSettings Player = new(config, isArenaLoaded, isEnabled);
    internal readonly GrabberSettings Grabber = new(config, isArenaLoaded, isEnabled);
    internal readonly EventLogSettings EventLog = new(config, isArenaLoaded, isEnabled);
    internal readonly ArenaSettings Arena = new(config, isArenaLoaded, isEnabled);
    internal readonly WaypointSettings Waypoint = new(config, isArenaLoaded, isEnabled);
    internal readonly VisualizationSettings Visualization = new(config, isArenaLoaded, isEnabled);
    internal readonly RenderingSettings Rendering = new(config, isArenaLoaded, isEnabled);
    internal readonly MapSettings Map = new(config, isArenaLoaded, isEnabled);
    internal readonly PreferenceSettings Preferences = new(config, isArenaLoaded, isEnabled);
    internal readonly FreecamSettings Freecam = new(config, isArenaLoaded, isEnabled);

    internal void LoadAll()
    {
        IsLoading = true;

        Player.Load();
        Arena.Load();
        EventLog.Load();
        Visualization.Load();
        Rendering.Load();
        Preferences.Load();
        Map.Load();
        Freecam.Load();

        IsLoading = false;
    }

    internal void FactoryReset()
    {
        IsLoading = true;

        Player.Reset();
        Grabber.Reset();
        Arena.Reset();
        EventLog.Reset();
        Visualization.Reset();
        Rendering.Reset();
        Preferences.Reset();
        Map.Reset();
        Freecam.Reset();

        IsLoading = false;

        Imperium.Reload();
    }
}