#region

using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using Imperium.API.Types;
using Imperium.Core;
using Imperium.Util;
using Librarium.Binding;
using JetBrains.Annotations;
using Librarium;
using Photon.Pun;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;

#endregion

namespace Imperium.Visualizers.Objects;

public class EntityGizmo : MonoBehaviour
{
    private const float visualizerTimeout = 2f;

    private EnemyParent enemyParent;

    private EntityGizmoConfig entityConfig;
    private Visualization visualization;

    private LineRenderer lastHeardNoiseLine;

    private GameObject lastHeardNoiseDot;
    // private LineRenderer targetLookLine;
    // private LineRenderer targetPlayerLine;

    private const int pathSegmentCount = 20;

    private LineRenderer pathLine;
    private readonly GameObject[] pathDots = new GameObject[pathSegmentCount];

    private EnemyVision enemyVision;
    private Transform staticParent;

    private GameObject coneStanding;
    private GameObject coneCrouching;
    private GameObject coneCrawling;

    private GameObject instantSphereStanding;
    private GameObject instantSphereCrouching;

    private float lastUpdateTime;

    private bool initializedVision;

    internal void NoiseVisualizerUpdate(Vector3 origin)
    {
        ImpGeometry.SetLinePositions(lastHeardNoiseLine, enemyParent.transform.position, origin);
        lastHeardNoiseLine.gameObject.SetActive(entityConfig.Hearing.Value);

        lastHeardNoiseDot.gameObject.SetActive(entityConfig.Hearing.Value);
        lastHeardNoiseDot.gameObject.transform.position = origin;
    }

    internal void Init(EntityGizmoConfig config, Visualization visualizer, EnemyParent entity, Transform parent)
    {
        entityConfig = config;
        visualization = visualizer;
        enemyParent = entity;
        enemyVision = entity.Enemy.Vision;
        staticParent = parent;

        pathLine = ImpGeometry.CreateLine(
            transform, 0.1f, true,
            startColor: new Color(0.58f, 1f, 0.6f),
            endColor: new Color(0.58f, 1f, 0.6f)
        );
        pathLine.gameObject.SetActive(false);

        for (var i = 0; i < pathSegmentCount; i++)
        {
            pathDots[i] = ImpGeometry.CreatePrimitive(
                PrimitiveType.Sphere, transform,
                color: new Color(1, 1, 1)
            );
            pathDots[i].SetActive(false);
        }

        lastHeardNoiseLine = ImpGeometry.CreateLine(
            transform, 0.03f, true,
            startColor: new Color(1f, 0.43f, 0.44f),
            endColor: new Color(1f, 0.43f, 0.44f)
        );
        lastHeardNoiseLine.gameObject.SetActive(false);

        lastHeardNoiseDot = ImpGeometry.CreatePrimitive(
            PrimitiveType.Sphere, transform,
            color: new Color(1, 1, 1),
            0.2f
        );
        lastHeardNoiseDot.SetActive(false);
    }

    private void InitVisionObjects(EnemyVision vision)
    {
        coneStanding = CreateCone("standing", vision.VisionDotStanding, vision.VisionDistance, ImpAssets.WireframeCyan);
        coneCrouching = CreateCone("crouching", vision.VisionDotCrouch, vision.VisionDistance, ImpAssets.WireframeGreen);
        coneCrawling = CreateCone("crawl", vision.VisionDotCrawl, vision.VisionDistance, ImpAssets.WireframeRed);

        instantSphereStanding = CreateSphere("standing", vision.VisionDistanceClose, ImpAssets.WireframeCyan);
        instantSphereCrouching = CreateSphere("crouching", vision.VisionDistanceCloseCrouch, ImpAssets.WireframeRed);

        initializedVision = true;
    }

    private (GameObject, GameObject) SelectActiveVisionObjects()
    {
        GameObject activeCone;
        GameObject activeSphere;

        if (enemyVision.Enemy.CurrentState == EnemyState.LookUnder)
        {
            activeCone = coneStanding;
            activeSphere = instantSphereStanding;

            coneCrouching.SetActive(false);
            coneCrawling.SetActive(false);
            instantSphereCrouching.SetActive(false);
        }
        else if (PlayerAvatar.instance.isCrawling && !PlayerAvatar.instance.isTumbling)
        {
            activeCone = coneCrawling;
            activeSphere = instantSphereCrouching;

            coneStanding.SetActive(false);
            coneCrouching.SetActive(false);
            instantSphereStanding.SetActive(false);
        }
        else if (PlayerAvatar.instance.isCrouching || PlayerAvatar.instance.isTumbling)
        {
            activeCone = coneCrouching;
            activeSphere = instantSphereCrouching;

            coneStanding.SetActive(false);
            coneCrawling.SetActive(false);
            instantSphereStanding.SetActive(false);
        }
        else
        {
            activeSphere = instantSphereStanding;

            activeCone = coneStanding;
            coneCrouching.SetActive(false);
            coneCrawling.SetActive(false);
            instantSphereCrouching.SetActive(false);
        }

        activeCone.SetActive(true);
        activeSphere.SetActive(true);

        return (activeCone, activeSphere);
    }

    private static GameObject CreateCone(string identifier, float dot, float distance, Material material)
    {
        var obj = new GameObject($"ImpVis_LoS_{identifier}");
        obj.AddComponent<MeshRenderer>().material = material;
        obj.AddComponent<MeshFilter>().mesh = Visualization.GenerateCone(
            Mathf.Acos(dot) * (180.0f / Mathf.PI)
        );
        obj.transform.localScale = Vector3.one * distance;

        return obj;
    }

    private static GameObject CreateSphere(string identifier, float radius, Material material)
    {
        var obj = ImpGeometry.CreatePrimitive(
            PrimitiveType.Sphere,
            parent: null,
            material: material,
            size: radius * 2,
            name: $"ImpVis_Instant_{identifier}"
        );

        return obj;
    }

    internal void VisionUpdate()
    {
        if (!initializedVision || !entityConfig.LineOfSight.Value) return;

        lastUpdateTime = Time.realtimeSinceStartup;

        var (activeCone, activeSphere) = SelectActiveVisionObjects();

        // Parent object to entity vision so it moves with entity if smooth animations are enabled
        activeCone.transform.SetParent(
            Imperium.Settings.Visualization.SmoothAnimations.Value ? enemyVision.VisionTransform : staticParent.transform
        );
        activeSphere.transform.SetParent(
            Imperium.Settings.Visualization.SmoothAnimations.Value ? enemyVision.VisionTransform : staticParent.transform
        );

        activeCone.transform.position = enemyVision.VisionTransform.position;
        activeCone.transform.rotation = Quaternion.LookRotation(enemyVision.VisionTransform.forward);

        activeSphere.transform.position = enemyVision.VisionTransform.position;
        activeSphere.transform.rotation = Quaternion.LookRotation(enemyVision.VisionTransform.forward);
    }

    private void Update()
    {
        if (!enemyParent)
        {
            Destroy(gameObject);
            return;
        }

        if (initializedVision)
        {
            if (Time.realtimeSinceStartup - lastUpdateTime > visualizerTimeout || !entityConfig.LineOfSight.Value)
            {
                coneStanding.SetActive(false);
                coneCrouching.SetActive(false);
                coneCrawling.SetActive(false);

                instantSphereStanding.SetActive(false);
                instantSphereCrouching.SetActive(false);
            }
        }

        DrawPathLines(entityConfig.Pathfinding.Value && enemyParent.enabled);
        DrawNoiseLine(entityConfig.Hearing.Value && enemyParent.enabled);

        // Vision object is sometimes not set right away so we just do it as soon as possible.
        if (!initializedVision && enemyParent.Enemy.Vision) InitVisionObjects(enemyParent.Enemy.Vision);
    }

    private void DrawNoiseLine(bool isShown)
    {
        // Disable noise line if setting has been turned off or the enemy is no longer investigating
        if (!isShown || enemyParent.Enemy.CurrentState != EnemyState.Investigate)
        {
            lastHeardNoiseLine.gameObject.SetActive(false);
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
        ImpGeometry.SetLinePositions(
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
                if (i == corners.Length - 1)
                {
                    pathDots[i].transform.localScale = Vector3.one * 0.4f;
                }
                else
                {
                    pathDots[i].transform.localScale = Vector3.one * 0.2f;
                }
            }
            else
            {
                pathDots[i].gameObject.SetActive(false);
            }
        }
    }

    private void OnDestroy()
    {
        if (initializedVision)
        {
            Destroy(coneStanding);
            Destroy(coneCrouching);
            Destroy(coneCrawling);
            Destroy(instantSphereStanding);
            Destroy(instantSphereCrouching);
        }
    }
}

internal class EntityGizmoConfig
{
    internal readonly string entityName;

    internal readonly ImpConfig<bool> Info;
    internal readonly ImpConfig<bool> Pathfinding;
    internal readonly ImpConfig<bool> Targeting;
    internal readonly ImpConfig<bool> LineOfSight;
    internal readonly ImpConfig<bool> Hearing;
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
        Targeting = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Targeting", escapedEntityName, false);
        LineOfSight = new ImpConfig<bool>(config, "Visualization.EntityGizmos.LineOfSight", escapedEntityName, false);
        Hearing = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Hearing", escapedEntityName, false);
        Custom = new ImpConfig<bool>(config, "Visualization.EntityGizmos.Custom", escapedEntityName, false);
    }
}