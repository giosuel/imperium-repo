using Imperium.API.Types;

namespace Imperium.API;

public static class EventLog
{
    /// <summary>
    /// Adds a new message to the event log.
    /// </summary>
    /// <param name="log">The event log message</param>
    public static void Add(EventLogMessage log)
    {
        APIHelpers.AssertImperiumReady();

        Imperium.EventLog.AddLog(log);
    }
}