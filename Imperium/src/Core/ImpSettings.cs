#region

using BepInEx.Configuration;
using Imperium.Core.Settings;

#endregion

namespace Imperium.Core;

/// <summary>
///     Contains all the bindings of the persistent settings of Imperium.
/// </summary>
public class ImpSettings(ConfigFile config)
{
    // Indication if settings are currently being loaded (to skip notifications and other things during loading)
    internal bool IsLoading { get; private set; }

    internal readonly PlayerSettings Player = new(config);
    internal readonly GrabberSettings Grabber = new(config);
    internal readonly EventLogSettings EventLog = new(config);
    internal readonly ArenaSettings Arena = new(config);
    internal readonly WaypointSettings Waypoint = new(config);
    internal readonly VisualizationSettings Visualization = new(config);
    internal readonly RenderingSettings Rendering = new(config);
    internal readonly MapSettings Map = new(config);
    internal readonly PreferenceSettings Preferences = new(config);
    internal readonly FreecamSettings Freecam = new(config);

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