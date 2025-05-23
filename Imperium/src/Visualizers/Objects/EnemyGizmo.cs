#region

using BepInEx.Configuration;
using Imperium.Util;
using Librarium;
using Librarium.Binding;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.Visualizers.Objects;

public class EnemyGizmo : MonoBehaviour
{
    private const float visualizerTimeout = 2f;

    private EnemyParent enemyParent;

    private EntityGizmoConfig entityConfig;

    private LineRenderer lastHeardNoiseLine;

    private GameObject lastHeardNoiseDot;

    private const int pathSegmentCount = 30;

    private LineRenderer pathLine;
    private readonly GameObject[] pathDots = new GameObject[pathSegmentCount];

    private EnemyVision enemyVision;

    private GameObject coneStanding;
    private GameObject coneCrouching;
    private GameObject coneCrawling;

    private GameObject proximitySphereStanding;
    private GameObject proximitySphereCrouching;

    private GameObject playerCloseSphere;
    private GameObject playerVeryCloseSphere;

    private float lastUpdateTimer;

    private bool hasInitializedVision;

    internal void Init(EntityGizmoConfig config, EnemyParent entity)
    {
        entityConfig = config;
        enemyParent = entity;
        enemyVision = entity.Enemy.Vision;

        pathLine = Geometry.CreateLine(
            transform, 0.04f, true,
            startColor: new Color(1f, 0.67f, 0.03f),
            endColor: new Color(1f, 0.67f, 0.03f),
            spawnDisabled: true
        );

        for (var i = 0; i < pathSegmentCount; i++)
        {
            pathDots[i] = Geometry.CreatePrimitive(
                PrimitiveType.Sphere, transform,
                material: ImpAssets.WireframeYellow,
                spawnDisabled: true
            );
        }

        lastHeardNoiseLine = Geometry.CreateLine(
            transform, 0.03f, true,
            startColor: new Color(1f, 0.43f, 0.44f),
            endColor: new Color(1f, 0.43f, 0.44f),
            spawnDisabled: true
        );

        lastHeardNoiseDot = Geometry.CreatePrimitive(
            PrimitiveType.Sphere, transform,
            color: new Color(1, 1, 1),
            0.2f,
            spawnDisabled: true
        );

        playerCloseSphere = Geometry.CreatePrimitive(
            PrimitiveType.Sphere, enemyParent.Enemy.transform,
            material: ImpAssets.WireframeOrange,
            size: 20f * 2,
            spawnDisabled: true
        );

        playerVeryCloseSphere = Geometry.CreatePrimitive(
            PrimitiveType.Sphere, enemyParent.Enemy.transform,
            material: ImpAssets.WireframeRed,
            size: 6f * 2,
            spawnDisabled: true
        );

        var enemyStatus = Instantiate(ImpAssets.EnemyStatus, transform);
        enemyStatus.AddComponent<EnemyStatus>().Init(enemyParent, entityConfig);

        if (enemyParent.Enemy.HasVision) InitVisionObjects(enemyParent.Enemy.Vision);
    }

    private void InitVisionObjects(EnemyVision vision)
    {
        coneStanding = CreateCone("standing", vision.VisionDotStanding, vision.VisionDistance, ImpAssets.WireframeCyan);
        coneCrouching = CreateCone("crouching", vision.VisionDotCrouch, vision.VisionDistance, ImpAssets.WireframeGreen);
        coneCrawling = CreateCone("crawl", vision.VisionDotCrawl, vision.VisionDistance, ImpAssets.WireframeRed);

        proximitySphereStanding = CreateSphere("standing", vision.VisionDistanceClose, ImpAssets.WireframeCyan);
        proximitySphereCrouching = CreateSphere("crouching", vision.VisionDistanceCloseCrouch, ImpAssets.WireframeRed);

        hasInitializedVision = true;
    }

    private (GameObject, GameObject) SelectActiveVisionObjects()
    {
        GameObject activeCone;
        GameObject activeSphere;

        if (enemyVision.Enemy.CurrentState == EnemyState.LookUnder)
        {
            activeCone = coneStanding;
            activeSphere = proximitySphereStanding;

            coneCrouching.SetActive(false);
            coneCrawling.SetActive(false);
            proximitySphereCrouching.SetActive(false);
        }
        else if (PlayerAvatar.instance.isCrawling && !PlayerAvatar.instance.isTumbling)
        {
            activeCone = coneCrawling;
            activeSphere = proximitySphereCrouching;

            coneStanding.SetActive(false);
            coneCrouching.SetActive(false);
            proximitySphereStanding.SetActive(false);
        }
        else if (PlayerAvatar.instance.isCrouching || PlayerAvatar.instance.isTumbling)
        {
            activeCone = coneCrouching;
            activeSphere = proximitySphereCrouching;

            coneStanding.SetActive(false);
            coneCrawling.SetActive(false);
            proximitySphereStanding.SetActive(false);
        }
        else
        {
            activeCone = coneStanding;
            activeSphere = proximitySphereStanding;

            coneCrouching.SetActive(false);
            coneCrawling.SetActive(false);
            proximitySphereCrouching.SetActive(false);
        }

        if (entityConfig.Vision.Value) activeCone.SetActive(true);
        if (entityConfig.Proximity.Value) activeSphere.SetActive(true);

        return (activeCone, activeSphere);
    }

    private GameObject CreateCone(string identifier, float dot, float distance, Material material)
    {
        var obj = new GameObject($"ImpVis_LoS_{identifier}");
        obj.transform.SetParent(transform);
        obj.SetActive(false);

        obj.AddComponent<MeshRenderer>().material = material;
        obj.AddComponent<MeshFilter>().mesh = Geometry.CreateCone(
            Mathf.Acos(dot) * (180f / Mathf.PI)
        );
        obj.transform.localScale = Vector3.one * distance;

        return obj;
    }

    private GameObject CreateSphere(string identifier, float radius, Material material)
    {
        var obj = Geometry.CreatePrimitive(
            PrimitiveType.Sphere,
            parent: transform,
            material: material,
            size: radius * 2,
            name: $"ImpVis_Instant_{identifier}",
            spawnDisabled: true
        );

        return obj;
    }

    internal void NoiseUpdate(Vector3 origin)
    {
        lastHeardNoiseLine.gameObject.SetActive(entityConfig.Hearing.Value);
        lastHeardNoiseDot.gameObject.SetActive(entityConfig.Hearing.Value);

        if (entityConfig.Hearing.Value)
        {
            Geometry.SetLinePositions(lastHeardNoiseLine, enemyParent.Enemy.transform.position, origin);
            lastHeardNoiseDot.gameObject.transform.position = origin;
        }
    }

    internal void VisionUpdate()
    {
        if (!hasInitializedVision || !entityConfig.Vision.Value && !entityConfig.Proximity.Value) return;

        lastUpdateTimer = visualizerTimeout;

        var (activeCone, activeSphere) = SelectActiveVisionObjects();

        if (entityConfig.Vision.Value)
        {
            activeCone.transform.SetParent(
                Imperium.Settings.Visualization.SmoothAnimations.Value ? enemyVision.VisionTransform : transform
            );
            activeCone.transform.position = enemyVision.VisionTransform.position;
            activeCone.transform.rotation = Quaternion.LookRotation(enemyVision.VisionTransform.forward);
        }

        if (entityConfig.Proximity.Value)
        {
            activeSphere.transform.SetParent(
                Imperium.Settings.Visualization.SmoothAnimations.Value ? enemyVision.VisionTransform : transform
            );

            activeSphere.transform.position = enemyVision.VisionTransform.position;
            activeSphere.transform.rotation = Quaternion.LookRotation(enemyVision.VisionTransform.forward);
        }
    }

    private void Update()
    {
        if (!enemyParent)
        {
            Destroy(gameObject);
            return;
        }

        if (hasInitializedVision)
        {
            if (!entityConfig.Vision.Value || lastUpdateTimer < 0)
            {
                coneStanding.SetActive(false);
                coneCrouching.SetActive(false);
                coneCrawling.SetActive(false);
            }

            if (!entityConfig.Proximity.Value || lastUpdateTimer < 0)
            {
                proximitySphereStanding.SetActive(false);
                proximitySphereCrouching.SetActive(false);
            }
        }

        DrawPathLines(entityConfig.Pathfinding.Value && enemyParent.enabled);
        DrawNoiseLine(entityConfig.Hearing.Value && enemyParent.enabled);
        DrawPlayerCloseSpheres();

        var uniqueId = SemiFunc.IsMultiplayer()
            ? enemyParent.photonView.ViewID
            : enemyParent.gameObject.GetInstanceID();

        if (!Imperium.ObjectManager.DisabledObjects.Value.Contains(uniqueId))
        {
            lastUpdateTimer -= Time.deltaTime;
        }
    }

    private void DrawNoiseLine(bool isShown)
    {
        // Disable noise line if setting has been turned off or the enemy is no longer investigating
        if (!isShown || enemyParent.Enemy.CurrentState != EnemyState.Investigate)
        {
            lastHeardNoiseLine.gameObject.SetActive(false);
            lastHeardNoiseDot.gameObject.SetActive(false);
        }
    }

    private void DrawPathLines(bool isShown)
    {
        if (!enemyParent || !enemyParent.Enemy.NavMeshAgent?.Agent)
        {
            foreach (var dot in pathDots) dot.gameObject.SetActive(false);
            pathLine.gameObject.SetActive(false);
            return;
        }

        pathLine.gameObject.SetActive(isShown);

        var corners = enemyParent.Enemy.NavMeshAgent.Agent.path.corners;
        Geometry.SetLinePositions(
            pathLine,
            [enemyParent.Enemy.transform.position, ..corners]
        );

        for (var i = 0; i < pathSegmentCount; i++)
        {
            if (i < corners.Length)
            {
                // Enable / Disable based on config
                pathDots[i].gameObject.SetActive(isShown);
                if (!isShown) continue;

                pathDots[i].transform.position = corners[i];
                pathDots[i].transform.localScale = Vector3.one * 0.15f;
            }
            else
            {
                pathDots[i].gameObject.SetActive(false);
            }
        }
    }

    private void DrawPlayerCloseSpheres()
    {
        var isShown = Imperium.Settings.Visualization.PlayerProximity.Value;

        playerCloseSphere.SetActive(isShown);

        // This is currently unused to we ignore it
        // playerVeryCloseSphere.SetActive(isShown);
    }

    private void OnDestroy()
    {
        if (hasInitializedVision)
        {
            Destroy(coneStanding);
            Destroy(coneCrouching);
            Destroy(coneCrawling);
            Destroy(proximitySphereStanding);
            Destroy(proximitySphereCrouching);
        }

        Destroy(playerCloseSphere);
        Destroy(playerVeryCloseSphere);
    }
}

internal class EntityGizmoConfig
{
    internal readonly string entityName;

    internal readonly ImpConfig<bool> Info;
    internal readonly ImpConfig<bool> Pathfinding;
    internal readonly ImpConfig<bool> Proximity;
    internal readonly ImpConfig<bool> Vision;
    internal readonly ImpConfig<bool> Hearing;
    internal readonly ImpConfig<bool> Vitality;
    internal readonly ImpConfig<bool> Custom;

    internal EntityGizmoConfig(string entityName, ConfigFile config)
    {
        this.entityName = entityName;

        var escapedEntityName = entityName
            .Replace("\"", "")
            .Replace("\'", "")
            .Replace("[", "")
            .Replace("]", "")
            .Replace("\n", "")
            .Replace("\t", "")
            .Replace("\\", "");

        Info = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Info", escapedEntityName, false);
        Pathfinding = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Pathfinding", escapedEntityName, false);
        Proximity = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Proximity", escapedEntityName, false);
        Vision = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Vision", escapedEntityName, false);
        Hearing = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Hearing", escapedEntityName, false);
        Vitality = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Vitality", escapedEntityName, false);
        Custom = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Custom", escapedEntityName, false);
    }
}