#region

using Imperium.Util;
using Librarium;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.Visualizers.Objects;

public class NoiseIndicator : MonoBehaviour
{
    private float timer;
    private bool isDone;

    private GameObject normalSphere;
    private GameObject spawnDecreaseSphere;
    private GameObject mutedSphere;

    internal void Awake()
    {
        normalSphere = Geometry.CreatePrimitive(
            PrimitiveType.Sphere,
            transform,
            ImpAssets.FresnelOrange
        );

        spawnDecreaseSphere = Geometry.CreatePrimitive(
            PrimitiveType.Sphere,
            transform,
            ImpAssets.FresnelRed
        );

        mutedSphere = Geometry.CreatePrimitive(
            PrimitiveType.Sphere,
            transform,
            ImpAssets.FresnelWhite
        );
    }

    internal void Activate(Vector3 position, float radius, int time, bool isMuted)
    {
        timer = time;
        isDone = false;

        transform.position = position;

        mutedSphere.transform.localScale = Vector3.one * radius;
        normalSphere.transform.localScale = Vector3.one * radius;
        spawnDecreaseSphere.transform.localScale = Vector3.one * radius;

        mutedSphere.SetActive(isMuted);
        normalSphere.SetActive(!isMuted && radius < 15);
        spawnDecreaseSphere.SetActive(!isMuted && radius >= 15);

        gameObject.SetActive(true);
    }

    internal void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void LateUpdate()
    {
        if (!Imperium.IsArenaLoaded.Value || Imperium.GameManager.IsGameLoading) return;

        if (timer >= 0)
        {
            timer -= Time.deltaTime;
        }
        else if (!isDone)
        {
            Deactivate();
            isDone = true;
        }
    }
}