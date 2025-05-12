using TMPro;
using UnityEngine;
using UnityEngine.InputSystem.Utilities;
using UnityEngine.UI;

namespace Imperium.Visualizers.Objects;

public class EnemyStatus : MonoBehaviour
{
    private Transform healthBar;
    private Image healthBarImage;
    private TMP_Text healthBarText;

    private Transform detectionBar;
    private Image detectionBarImage;
    private TMP_Text detectionBarText;

    private EnemyParent enemy;
    private RectTransform canvasRect;
    private RectTransform panelRect;

    private Transform origin;
    private bool hasVision;
    private Vector3 panelOffset;

    private Gradient healthGradient;
    private EntityGizmoConfig config;

    private bool leFunniDuck;

    private void Awake()
    {
        canvasRect = transform.GetComponent<RectTransform>();
        panelRect = transform.Find("Panel").GetComponent<RectTransform>();
        panelRect.gameObject.SetActive(false);

        healthBar = panelRect.Find("Health/Container/Bar");
        healthBarImage = healthBar.GetComponent<Image>();
        healthBarText = panelRect.Find("Health/Container/Text").GetComponent<TMP_Text>();

        detectionBar = panelRect.Find("Detection/Container/Bar");
        detectionBarImage = detectionBar.GetComponent<Image>();
        detectionBarText = panelRect.Find("Detection/Container/Text").GetComponent<TMP_Text>();
        detectionBarText.text = "";

        SetHealthGradient();
    }

    private void SetHealthGradient()
    {
        var colorKeys = new GradientColorKey[]
        {
            new(Color.red, 0.3f),
            new(Color.yellow, 0.6f),
            new(Color.green, 1f)
        };

        var alphaKeys = new GradientAlphaKey[]
        {
            new() { alpha = 1.0f, time = 0.0f },
            new() { alpha = 1.0f, time = 1.0f }
        };

        healthGradient = new Gradient();
        healthGradient.SetKeys(colorKeys, alphaKeys);
    }

    internal void Init(EnemyParent enemyParent, EntityGizmoConfig enemyConfig)
    {
        enemy = enemyParent;
        config = enemyConfig;

        hasVision = enemy.Enemy.HasVision;
        origin = enemy.Enemy.HasVision ? enemy.Enemy.Vision.VisionTransform : enemy.Enemy.transform;
        panelOffset = GetPanelOffset(enemy);

        leFunniDuck = enemy.enemyName == "Apex Predator" && Random.Range(0, 100) < 5;

        // Disable vision bar if enemy doesn't see
        if (!hasVision) panelRect.Find("Detection").gameObject.SetActive(false);
    }

    private float currentHealthValue;
    private const float smoothSpeed = 5f;

    private void Update()
    {
        var targetHealth = Mathf.Min((float)enemy.Enemy.Health.healthCurrent / enemy.Enemy.Health.health, 1);
        currentHealthValue = Mathf.Lerp(currentHealthValue, targetHealth, Time.deltaTime * smoothSpeed);
        healthBar.localScale = detectionBar.localScale with { x = currentHealthValue };
        healthBarImage.color = healthGradient.Evaluate(currentHealthValue);
        healthBarText.text = $"{enemy.Enemy.Health.healthCurrent} HP";

        if (hasVision)
        {
            int maxVisions;

            if (enemy.Enemy.CurrentState == EnemyState.LookUnder)
            {
                maxVisions = enemy.Enemy.Vision.VisionsToTrigger + 1;
            }
            else if (PlayerAvatar.instance.isCrawling && !PlayerAvatar.instance.isTumbling)
            {
                maxVisions = enemy.Enemy.Vision.VisionsToTriggerCrawl + 1;
            }
            else if (PlayerAvatar.instance.isCrouching || PlayerAvatar.instance.isTumbling)
            {
                maxVisions = enemy.Enemy.Vision.VisionsToTriggerCrouch + 1;
            }
            else
            {
                maxVisions = enemy.Enemy.Vision.VisionsToTrigger + 1;
            }

            var currentVisions = enemy.Enemy.Vision.VisionsTriggered.GetValueOrDefault(Imperium.Player.photonView.ViewID);
            var detectionPercentage = Mathf.Min(currentVisions / (float)maxVisions, 1);
            detectionBar.localScale = detectionBar.localScale with { x = detectionPercentage };

            detectionBarText.text = currentVisions >= maxVisions ?
                leFunniDuck ? "Ducktected!" : "Detected!"
                : currentVisions == 0
                    ? "Idle"
                    : $"{Mathf.Min(currentVisions, maxVisions)}/{maxVisions}";
        }
    }

    private void LateUpdate()
    {
        var camera = Imperium.ActiveCamera.Value;
        if (!camera || !enemy || !enemy.Spawned || !config.Vitality.Value)
        {
            panelRect.gameObject.SetActive(false);
            return;
        }

        var worldPosition = origin.position + panelOffset;
        canvasRect.transform.position = worldPosition;
        panelRect.gameObject.SetActive(true);

        var directionToCamera = -(Imperium.ActiveCamera.Value.transform.position - panelRect.transform.position);
        var lookRotation = Quaternion.LookRotation(directionToCamera, Vector3.up);
        panelRect.transform.rotation = lookRotation;
    }

    private static Vector3 GetPanelOffset(EnemyParent enemyParent)
    {
        if (enemyParent.GetComponentInChildren<EnemyCeilingEye>() != null) return Vector3.down * 0.2f;
        if (enemyParent.GetComponentInChildren<EnemyHeadController>() != null) return Vector3.up * 0.6f;
        if (enemyParent.GetComponentInChildren<EnemyRobe>() != null) return Vector3.up * 0.4f;
        if (enemyParent.GetComponentInChildren<EnemyHunter>() != null) return Vector3.up * 2.4f;

        return Vector3.up * 0.2f;
    }
}