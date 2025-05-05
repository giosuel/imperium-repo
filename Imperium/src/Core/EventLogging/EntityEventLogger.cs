#region

using Imperium.API.Types;
using Librarium;
using UnityEngine;

#endregion

namespace Imperium.Core.EventLogging;

internal class EntityEventLogger(ImpEventLog log)
{
    private void LogEntityEvent(EnemyParent instance, string message, string action = null, params EventLogDetail[] details)
    {
        log.AddLog(new EventLogMessage
        {
            ObjectName = $"{instance.enemyName}",
            Message = message,
            DetailsTitle = action,
            Details = details,
            Type = EventLogType.Entity
        });
    }

    internal void Awake(EnemyParent instance)
    {
        LogEntityEvent(instance, "Started existing.", action: "Awake");
    }

    internal void Spawn(EnemyParent instance)
    {
        LogEntityEvent(
            instance, "Enemy has spawned.", action: "Spawn",
            new EventLogDetail
            {
                Title = "Spawned Timer",
                Text = Formatting.FormatSecondsMinutes(instance.SpawnedTimer)
            },
            new EventLogDetail
            {
                Title = "Spawned Pause Timer",
                Text = Formatting.FormatSecondsMinutes(instance.spawnedTimerPauseTimer)
            }
        );
    }

    internal void Despawn(EnemyParent instance)
    {
        LogEntityEvent(
            instance, "Enemy has despawned.", action: "Despawn",
            new EventLogDetail
            {
                Title = "Despawned Timer",
                Text = Formatting.FormatSecondsMinutes(instance.DespawnedTimer)
            }
        );
    }

    internal void Teleport(EnemyParent instance, Vector3 teleportPosition)
    {
        LogEntityEvent(
            instance, "Enemy has teleported to a new location.", action: "Teleport",
            new EventLogDetail

            {
                Title = "Destination",
                Text = Formatting.FormatVector(teleportPosition)
            }
        );
    }

    internal void Investigating(EnemyStateInvestigate instance, Vector3 position)
    {
        // LogEntityEvent(
        //     instance.Enemy.EnemyParent, "Enemy has started to investigate.", action: "Investigate",
        //     new EventLogDetail
        //     {
        //         Title = "Investigation Position",
        //         Text = $"{Formatting.FormatVector(position)}"
        //     },
        //     new EventLogDetail
        //     {
        //         Title = "Investigation Point",
        //         Text = $"{Formatting.FormatVector(instance.InvestigateLevelPoint.transform.position)}"
        //     }
        // );
    }
}