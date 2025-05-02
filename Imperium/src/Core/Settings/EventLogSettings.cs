#region

using BepInEx.Configuration;
using Librarium;
using Librarium.Binding;

#endregion

namespace Imperium.Core.Settings;

internal class EventLogSettings(ConfigFile config, IBinding<bool> isSceneLoaded, IBinding<bool> isEnabled)
    : SettingsContainer(config)
{
    internal readonly ImpConfig<bool> EntityLogs = new(config, "EventLog", "Entity", true);
    internal readonly ImpConfig<bool> PlayerLogs = new(config, "EventLog", "Player", true);
    internal readonly ImpConfig<bool> GameLogs = new(config, "EventLog", "Game", true);
    internal readonly ImpConfig<bool> CustomLogs = new(config, "EventLog", "Custom", true);
}