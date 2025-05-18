using Imperium.Core.Lifecycle;
using Imperium.Util;
using UnityEngine;

namespace Imperium.Console.Commands;

/// <summary>
/// Console command to spawn a game object.
/// </summary>
/// <param name="name">The display name of the object to spawn</param>
/// <param name="objectName">The internal name of the object to spawn</param>
/// <param name="spawnType">The type of object to spawn</param>
internal sealed class ImpCommandSpawn(
    string name,
    string objectName,
    SpawnObjectType spawnType
) : ImpCommand(name)
{
    internal override string DisplayType => $"Spawn {spawnType.ToString()}";
    internal override Sprite Icon => customIcon ?? ImpAssets.IconCommandSpawn;

    internal override bool Execute(ConsoleQuery query)
    {
        if (query.Args.Length < 1 || !int.TryParse(query.Args[0], out var amount))
        {
            amount = 1;
        }

        if (query.Args.Length < 2 || !int.TryParse(query.Args[1], out var value))
        {
            value = -1;
        }

        if (Imperium.Freecam.IsFreecamEnabled.Value || Imperium.PlayerManager.IsFlying.Value)
        {
            var originTransform = Imperium.Freecam.IsFreecamEnabled.Value
                ? Imperium.Freecam.transform
                : PlayerAvatar.instance.localCamera.transform;

            Imperium.PositionIndicator.Activate(
                position => Imperium.ObjectManager.Spawn(objectName, spawnType, position, amount, value),
                originTransform,
                castGround: false
            );
        }
        else
        {
            var playerTransform = PlayerAvatar.instance.localCamera.transform;
            var spawnPosition = playerTransform.position + playerTransform.forward * 3f;

            var ray = new Ray(playerTransform.position, playerTransform.forward);
            if (Physics.Raycast(ray, out var hitInfo))
            {
                if (Vector3.Distance(playerTransform.position, hitInfo.point) <
                    Vector3.Distance(playerTransform.position, spawnPosition))
                {
                    spawnPosition = hitInfo.point;
                }
            }

            Imperium.ObjectManager.Spawn(objectName, spawnType, spawnPosition, amount, value);
        }

        return true;
    }

    internal override string GetDisplayName(ConsoleQuery query) => $"Spawn {objectName}";

    internal override bool IsEnabled() => Imperium.IsGameLevel.Value;
}