# Event Log API

The event log is a database of events that can be fired by the game or any mod using it.

![Event Log](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/event-log.png?raw=true)

## Usage Guide
The following example adds a new log to the event log.

```csharp
Imperium.API.EventLog.Add(new EventLogMessage
{
    ObjectName = "<objectName>",
    Message = "<message>",
    DetailsTitle = "<action>",
    Details =
    [
        new EventLogDetail
        {
            Title = "<detailTitle>",
            Text = "<detailText>"
        }
    ],
    Type = EventLogType.Custom
});
```

### Example

The following example logs an event every time an enemy spawns.

```csharp
using Imperium.API.Types;

[HarmonyPatch(typeof(EnemyParent))]
private static void EnemyParentPatch
{
    [HarmonyPostfix]
    [HarmonyPatch("SpawnRPC")]
    private static void SpawnPatch(EnemyParent __instance)
    {
        Imperium.API.EventLog.Add(new EventLogMessage
        {
            ObjectName = __instance.enemyName,
            Message = "This enemy just spawned",
            DetailsTitle = "Spawn",
            Details =
            [
                new EventLogDetail
                {
                    Title = "Spawned Timer",
                    Text = Formatting.FormatSecondsMinutes(
                        __instance.SpawnedTimer
                    )
                },
                new EventLogDetail
                {
                    Title = "Spawned Pause Timer",
                    Text = Formatting.FormatSecondsMinutes(
                        __instance.spawnedTimerPauseTimer
                    )
                }
            ],
            Type = EventLogType.Entity
        });
    }
}
```

This will look something like this in the game.

![Event Log Example](https://github.com/giosuel/imperium-repo/blob/development/assets/screenshots/event-log_example.png?raw=true)


### Event Types

Imperium supports the following event types. The user can filter event logs by log type in the Event Log UI.

```csharp
public enum EventLogType
{
    Entity,
    Player,
    Game,
    Custom
}
```