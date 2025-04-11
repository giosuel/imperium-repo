#region

using System.Collections.Generic;
using Imperium.Util;
using Librarium.Binding;

#endregion

namespace Imperium.Core.EventLogging;

public class ImpEventLog
{
    public readonly ImpBinding<List<EventLogMessage>> Log = new([]);

    internal readonly EntityEventLogger EntityEvents;
    internal readonly PlayerEventLogger PlayerEvents;
    internal readonly GameEventLogger GameEvents;

    private EventLogMessage latestLog;

    internal ImpEventLog()
    {
        EntityEvents = new EntityEventLogger(this);
        PlayerEvents = new PlayerEventLogger(this);
        GameEvents = new GameEventLogger(this);
    }

    public void AddLog(EventLogMessage log)
    {
        if (latestLog.ObjectName == log.ObjectName && latestLog.Message == log.Message)
        {
            latestLog.Count++;
            Log.Refresh();
            return;
        }

        // log.Time = (Imperium.TimFormatting.FormatDayTimeeOfDay.currentDayTime);
        log.Time = "00:00:69";
        // log.Day = Imperium.StartOfRound.gameStats.daysSpent;
        log.Day = 1;

        latestLog = log;
        Log.Value.Add(log);
        Log.Refresh();
    }
}