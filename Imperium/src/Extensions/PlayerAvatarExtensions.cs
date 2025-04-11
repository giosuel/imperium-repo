using UnityEngine;

namespace Imperium.Extensions;

public static class PlayerAvatarExtensions
{
    public static Vector3 GetFloorPosition(this PlayerAvatar avatar)
    {
        return avatar.physObjectStander.transform.position;
    }
}