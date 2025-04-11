#region

using System.Collections.Generic;
using BepInEx.Configuration;
using Imperium.API.Types;
using Librarium.Binding;
using Imperium.Visualizers.Objects;
using UnityEngine;

#endregion

namespace Imperium.Visualizers;

internal class PlayerGizmos : BaseVisualizer<IReadOnlyCollection<PlayerAvatar>, PlayerGizmo>
{
    internal readonly Dictionary<PlayerAvatar, PlayerGizmoConfig> PlayerInfoConfigs = [];

    internal PlayerGizmos(
        Transform parent,
        IBinding<IReadOnlyCollection<PlayerAvatar>> objectsBinding, ConfigFile config
    ) : base(parent, objectsBinding)
    {
        foreach (var player in Imperium.GameDirector.PlayerList)
        {
            PlayerInfoConfigs[player] = new PlayerGizmoConfig(player.playerName, config);
        }
    }

    protected override void OnRefresh(IReadOnlyCollection<PlayerAvatar> objects)
    {
        ClearObjects();

        foreach (var player in objects)
        {
            if (!visualizerObjects.ContainsKey(player.GetInstanceID()))
            {
                var playerGizmoObject = new GameObject($"Imp_PlayerInfo_{player.GetInstanceID()}");
                playerGizmoObject.transform.SetParent(parent);

                var playerGizmo = playerGizmoObject.AddComponent<PlayerGizmo>();
                if (!PlayerInfoConfigs.TryGetValue(player, out var playerInfoConfig))
                {
                    Imperium.IO.LogInfo("[ERR] Player was not found, no config loaded for insight.");
                    continue;
                }

                playerGizmo.Init(playerInfoConfig, player.GetComponent<PlayerAvatar>());
                visualizerObjects[player.GetInstanceID()] = playerGizmo;
            }
        }
    }

    internal void PlayerNoiseUpdate(PlayerAvatar player, float range)
    {
        visualizerObjects[player.GetInstanceID()].NoiseUpdate(range);
    }

    /*
     * Player Hit Ground
     * Player Footstep
     * Player Voice
     */
}