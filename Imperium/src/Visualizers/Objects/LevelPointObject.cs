#region

using UnityEngine;

#endregion

namespace Imperium.Visualizers.Objects;

public class LevelPointObject : MonoBehaviour
{
    private void Update()
    {
        transform.LookAt(Imperium.ActiveCamera.Value.transform.position with
        {
            y = transform.position.y
        });
    }
}