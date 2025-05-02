#region

using System;
using Imperium.API.Types.Networking;
using Imperium.Util;
using RepoSteamNetworking.API;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ObjectExplorer.ObjectListEntry;

internal static class ObjectEntryGenerator
{
    internal static bool CanDestroy(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Player => false,
        ObjectType.ExtractionPoint => false,
        _ => true
    };

    internal static bool CanDespawn(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Entity when entry.component is EnemyParent { Spawned: true } => true,
        _ => false
    };

    internal static bool CanSpawn(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Entity when entry.component is EnemyParent { Spawned: false } => true,
        _ => false
    };

    internal static bool CanComplete(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.ExtractionPoint => true,
        _ => false
    };

    internal static bool CanKill(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Player when entry.component is PlayerAvatar { gameObject.activeSelf: true } => true,
        _ => false
    };

    internal static bool CanRevive(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Player when entry.component is PlayerAvatar { gameObject.activeSelf: false } => true,
        _ => false
    };

    internal static bool CanToggle(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Entity => true,
        _ => false
    };

    internal static void DestroyObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.Valuable:
            case ObjectType.Item:
            case ObjectType.Entity:
                Imperium.ObjectManager.DespawnObject(new ObjectDespawnRequest
                {
                    ViewId = entry.View.ViewID
                });
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void DespawnObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.Entity:
                ((EnemyParent)entry.component).Despawn();
                break;
            case ObjectType.Player:
            case ObjectType.Item:
            case ObjectType.Valuable:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void SpawnObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.Entity:
                ((EnemyParent)entry.component).Spawn();
                break;
            case ObjectType.Player:
            case ObjectType.Item:
            case ObjectType.Valuable:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void CompleteObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.ExtractionPoint:
                Imperium.ObjectManager.CompleteExtraction(new ExtractionCompleteRequest
                {
                    ViewId = ((ExtractionPoint)entry.component).photonView.ViewID
                });
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void KillObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.Player when entry.component is PlayerAvatar { gameObject.activeSelf: true } player:
                Imperium.PlayerManager.KillPlayer(player.GetSteamId());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void ReviveObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.Player when entry.component is PlayerAvatar { gameObject.activeSelf: false } player:
                Imperium.PlayerManager.RevivePlayer(player.GetSteamId());
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void ToggleObject(ObjectEntry entry, bool isActive)
    {
        switch (entry.Type)
        {
            case ObjectType.Entity:
                var entity = (EnemyParent)entry.component;
                entity.enabled = isActive;
                entity.Enemy.gameObject.SetActive(isActive);
                if (entity.Enemy?.NavMeshAgent?.Agent != null) entity.Enemy.NavMeshAgent.Agent.isStopped = !isActive;
                // foreach (var animator in entity.GetComponentsInChildren<Animator>()) animator.enabled = isActive;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void TeleportObjectHere(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.Player:
                Imperium.PositionIndicator.Activate(position =>
                {
                    Imperium.PlayerManager.TeleportPlayer(new TeleportPlayerRequest
                    {
                        PlayerId = ((PlayerAvatar)entry.component).GetSteamId(),
                        Destination = position
                    });
                }, Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null, castGround: true);
                Imperium.Interface.Close();
                break;
            case ObjectType.Entity:
                Imperium.PositionIndicator.Activate(position =>
                {
                    Imperium.ObjectManager.TeleportEnemy(new EnemyTeleportRequest
                    {
                        Destination = position,
                        ViewId = entry.View.ViewID
                    });
                }, castGround: false);
                break;
            case ObjectType.Valuable:
                Imperium.PositionIndicator.Activate(position =>
                {
                    Imperium.ObjectManager.TeleportValuable(new ValuableTeleportRequest
                    {
                        Destination = position,
                        ViewId = entry.View.ViewID
                    });
                }, castGround: true);
                break;
            case ObjectType.ExtractionPoint:
            case ObjectType.Item:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void IntervalUpdate(ObjectEntry entry)
    {
        switch (entry.Type)
        {
            case ObjectType.ExtractionPoint:
                var point = (ExtractionPoint)entry.component;
                entry.completeButton.interactable = point.currentState != ExtractionPoint.State.Complete;
                break;
            case ObjectType.Entity:
            case ObjectType.Player:
            case ObjectType.Item:
            case ObjectType.Valuable:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    internal static void InitObject(ObjectEntry entry)
    {
        switch (entry.Type)
        {
        }
    }

    internal static string GetObjectName(ObjectEntry entry) => entry.Type switch
    {
        ObjectType.Player => GetPlayerName((PlayerAvatar)entry.component),
        ObjectType.ExtractionPoint => ((ExtractionPoint)entry.component).name,
        ObjectType.Entity => GetEnemyName((EnemyParent)entry.component),
        ObjectType.Item => ((ItemAttributes)entry.component).itemName,
        ObjectType.Valuable => GetValuableName((ValuableObject)entry.component),
        _ => entry.component.name
    };

    internal static Vector3 GetTeleportPosition(ObjectEntry entry) => entry.Type switch
    {
        // ObjectType.Vent => ((EnemyVent)entry.component).floorNode.position,
        ObjectType.Entity => ((EnemyParent)entry.component).Enemy.transform.position,
        _ => entry.containerObject.transform.position
    };

    internal static GameObject GetContainerObject(ObjectEntry entry) => entry.Type switch
    {
        _ => entry.component.gameObject
    };

    private static string GetValuableName(ValuableObject valuable)
    {
        return valuable.name.Replace("(Clone)", "").Replace("Valuable ", "");
    }

    private static string GetPlayerName(PlayerAvatar player)
    {
        var playerName = player.playerName;
        if (string.IsNullOrEmpty(playerName)) playerName = $"Player {player.GetInstanceID()}";

        // Check if player is also using Imperium
        if (Imperium.Networking.ImperiumUsers.Value.Contains(player.GetSteamId()))
        {
            playerName = $"[I] {playerName}";
        }

        if (player.GetSteamId() == RepoSteamNetwork.GetCurrentLobby().Owner.Id)
        {
            playerName = RichText.Bold(playerName);
        }

        return !player.gameObject.activeSelf ? RichText.Strikethrough(playerName) : playerName;
    }

    private static string GetEnemyName(EnemyParent enemy)
    {
        return enemy.Spawned ? enemy.enemyName : RichText.Strikethrough(enemy.enemyName);
    }
}