#region

using Imperium.Core;
using Imperium.Extensions;
using Imperium.Interface.Common;
using Imperium.Types;
using Librarium;
using Librarium.Binding;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.MapUI;

internal class
    MapUI : BaseUI
{
    private GameObject compass;
    private Transform compassNorth;
    private Transform compassEast;
    private Transform compassSouth;
    private Transform compassWest;

    private ImpSlider farClipSlider;
    private ImpSlider nearClipSlider;

    private Transform target;
    private Vector3 cameraViewOrigin = Vector3.zero;

    private Vector3 cameraTargetRotation;
    private float snapBackAnimationTimer;

    private float mouseOffsetX;
    private float mouseOffsetY;

    private RawImage textureFrame;

    private readonly ImpBinding<PlayerAvatar> selectedPlayer = new(null);
    private readonly ImpBinding<EnemyParent> selectedEntity = new(null);
    private readonly ImpBinding<(GameObject, string)> selectedMapHazard = new();

    protected override void InitUI()
    {
        Imperium.InputBindings.BaseMap.Reset.performed += OnMapReset;
        Imperium.InputBindings.BaseMap.Minimap.performed += OnMinimapToggle;

        InitRenderTexture();
        InitCompass();
        InitSliders();
        InitMapSettings();
        InitTargetSelection();

        // Init layer selector and bind the layer mask
        var layerSelector = container.Find("LayerSelector").gameObject.AddComponent<LayerSelector.LayerSelector>();
        layerSelector.InitUI(theme);
        layerSelector.Bind(new ImpBinding<bool>(true), Imperium.Settings.Map.CameraLayerMask);

        Imperium.Settings.Map.RotationLock.OnTrigger += OnRotationLockChange;

        selectedPlayer.Set(Imperium.Player);
        Imperium.IsLevelLoaded.OnTrue += () => selectedPlayer.Set(Imperium.Player);
    }

    private void OnDrag(Vector3 delta)
    {
        if (Imperium.InputBindings.BaseMap.MapRotate.IsPressed())
        {
            mouseOffsetX += delta.x * 0.25f;
            mouseOffsetY -= delta.y * 0.25f;
            mouseOffsetY = Mathf.Clamp(mouseOffsetY, -89.9f, 89.9f);

            // Change clipping to global when the perspective is changed
            if (delta.y > 1)
            {
                Imperium.Map.CameraNearClip.Set(ImpConstants.DefaultMapCameraNearClipFreeLook);
                Imperium.Map.CameraFarClip.Set(ImpConstants.DefaultMapCameraFarClipFreeLook);
            }

            // Set the animation timer to 0 to interrupt the slide animation
            snapBackAnimationTimer = 0;
            cameraTargetRotation = new Vector3(mouseOffsetX, mouseOffsetY, 0);
        }
        else if (Imperium.InputBindings.BaseMap.MapPan.IsPressed())
        {
            var inputVector = new Vector3(
                -delta.x * 0.0016f * Imperium.Settings.Map.CameraZoom.Value,
                -delta.y * 0.0016f * Imperium.Settings.Map.CameraZoom.Value,
                0
            );
            inputVector = Imperium.Map.Camera.transform.TransformDirection(inputVector);

            cameraViewOrigin += inputVector;

            // De-select current entity / player when switching to pan mode
            selectedEntity.Set(null);
            selectedPlayer.Set(null);

            if (target)
            {
                // "Apply" target rotation when enabling / disabling rotation lock
                if (Imperium.Settings.Map.RotationLock.Value)
                {
                    mouseOffsetX += target.rotation.eulerAngles.y;
                }

                target = null;
            }

            // Set the animation timer to 0 to interrupt the slide animation
            snapBackAnimationTimer = 0;
            cameraTargetRotation = new Vector3(mouseOffsetX, mouseOffsetY, 0);
        }
    }

    protected override void OnThemePrimaryUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container,
            new StyleOverride("LayerSelector", Variant.BACKGROUND),
            new StyleOverride("LayerSelector/Border", Variant.DARKER),
            new StyleOverride("LayerSelector/ScrollView/Scrollbar", Variant.DARKEST),
            new StyleOverride("LayerSelector/ScrollView/Scrollbar/SlidingArea/Handle", Variant.LIGHTER),
            new StyleOverride("MapBorder", Variant.DARKER),
            new StyleOverride("MapSettings", Variant.BACKGROUND),
            new StyleOverride("MapSettings/Border", Variant.DARKER),
            new StyleOverride("TargetSelection", Variant.BACKGROUND),
            new StyleOverride("TargetSelection/Border", Variant.DARKER),
            // Compass
            new StyleOverride("Compass", Variant.FOREGROUND),
            new StyleOverride("Compass/Icon", Variant.FOREGROUND),
            // Far Clip Slider
            new StyleOverride("FarClip", Variant.BACKGROUND),
            new StyleOverride("FarClip/Border", Variant.DARKER),
            // Near Clip Slider
            new StyleOverride("NearClip", Variant.BACKGROUND),
            new StyleOverride("NearClip/Border", Variant.DARKER),
            // Zoom Slider
            new StyleOverride("ZoomSlider", Variant.BACKGROUND),
            new StyleOverride("ZoomSlider/Border", Variant.DARKER)
        );
        ImpThemeManager.StyleText(
            themeUpdate,
            container,
            new StyleOverride("Compass/North", Variant.FOREGROUND),
            new StyleOverride("Compass/East", Variant.FOREGROUND),
            new StyleOverride("Compass/South", Variant.FOREGROUND),
            new StyleOverride("Compass/West", Variant.FOREGROUND)
        );
    }

    private void OnRotationLockChange() => MoveCameraToTarget(target);

    private static void OnMinimapToggle(InputAction.CallbackContext _)
    {
        if (Imperium.Interface.IsOpen() || MenuManager.instance.IsOpen() || ChatManager.instance.IsOpen()) return;

        Imperium.Settings.Map.MinimapEnabled.Set(!Imperium.Settings.Map.MinimapEnabled.Value);
    }

    private void OnMapReset(InputAction.CallbackContext _) => OnMapReset();

    private void OnMapReset()
    {
        MoveCameraToTarget(PlayerAvatar.instance.localCamera.transform);
    }

    private void InitCompass()
    {
        compass = container.Find("Compass").gameObject;
        compass.SetActive(Imperium.Settings.Map.CompassEnabled.Value);
        compass.gameObject.AddComponent<ImpInteractable>().onClick += OnMapReset;
        Imperium.Settings.Map.CompassEnabled.OnUpdate += compass.SetActive;

        compassNorth = compass.transform.Find("North");
        compassEast = compass.transform.Find("East");
        compassSouth = compass.transform.Find("West");
        compassWest = compass.transform.Find("South");
    }

    private void InitSliders()
    {
        ImpSlider.Bind(
            path: "ZoomSlider",
            container: container,
            valueBinding: Imperium.Settings.Map.CameraZoom,
            minValue: 1,
            maxValue: 100,
            valueUnit: "x",
            handleFormatter: value => $"{Mathf.RoundToInt(value)}",
            theme: theme
        );
        container.Find("ZoomSlider/MinIcon").gameObject.AddComponent<ImpInteractable>()
            .onClick += () => Imperium.Settings.Map.CameraZoom.Set(1);
        container.Find("ZoomSlider/MaxIcon").gameObject.AddComponent<ImpInteractable>()
            .onClick += () => Imperium.Settings.Map.CameraZoom.Set(100);

        nearClipSlider = ImpSlider.Bind(
            path: "FarClip",
            container: container,
            valueBinding: Imperium.Map.CameraNearClip,
            minValue: -50,
            maxValue: 200,
            handleFormatter: value => $"{Mathf.RoundToInt(value)}",
            playClickSound: false,
            theme: theme
        );
        nearClipSlider.gameObject.SetActive(!Imperium.Settings.Map.AutoClipping.Value);
        Imperium.Settings.Map.AutoClipping.OnUpdate += value => nearClipSlider.gameObject.SetActive(!value);

        farClipSlider = ImpSlider.Bind(
            path: "NearClip",
            container: container,
            valueBinding: Imperium.Map.CameraFarClip,
            minValue: -50,
            maxValue: 200,
            handleFormatter: value => $"{Mathf.RoundToInt(value)}",
            playClickSound: false,
            theme: theme
        );
        farClipSlider.gameObject.SetActive(!Imperium.Settings.Map.AutoClipping.Value);
        Imperium.Settings.Map.AutoClipping.OnUpdate += value => farClipSlider.gameObject.SetActive(!value);
    }

    private void InitMapSettings()
    {
        ImpToggle.Bind("MapSettings/MinimapEnabled", container, Imperium.Settings.Map.MinimapEnabled, theme: theme);
        ImpToggle.Bind("MapSettings/CompassEnabled", container, Imperium.Settings.Map.CompassEnabled, theme: theme);
        ImpToggle.Bind(
            "MapSettings/RotationLock",
            container,
            Imperium.Settings.Map.RotationLock,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Tooltip = tooltip,
                Description = "Whether the camera is clamped\nto the target's rotation",
            }
        );
        ImpToggle.Bind(
            "MapSettings/UnlockView",
            container,
            Imperium.Settings.Map.UnlockView,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Tooltip = tooltip,
                Description = "When off, the camera resets to a 45 angle.\nWhen on, the camers resets to top-down view."
            }
        );
        ImpToggle.Bind("MapSettings/AutoClipping", container, Imperium.Settings.Map.AutoClipping, theme: theme);
        ImpButton.Bind(
            "MapSettings/MinimapSettings",
            container,
            () => Imperium.Interface.Open<MinimapSettings>(),
            theme: theme
        );
    }

    private void InitTargetSelection()
    {
        selectedPlayer.OnUpdate += player =>
        {
            if (!player) return;

            selectedEntity.Set(null);
            selectedMapHazard.Set(default);

            MoveCameraToTarget(player.localCamera.transform);
        };

        selectedEntity.OnUpdate += entity =>
        {
            if (!entity) return;

            selectedPlayer.Set(null);
            selectedMapHazard.Set(default);

            target = entity.Enemy.transform;
            MoveCameraToTarget(entity.Enemy.transform);
        };

        ImpMultiSelect.Bind(
            "TargetSelection/Players",
            container,
            selectedPlayer,
            Imperium.ObjectManager.CurrentPlayers,
            player => player.playerName,
            theme: theme
        );

        ImpMultiSelect.Bind(
            "TargetSelection/Entities",
            container,
            selectedEntity,
            Imperium.ObjectManager.CurrentLevelEntities,
            entity => entity.enemyName,
            theme: theme
        );
    }

    private void MoveCameraToTarget(Transform newTarget)
    {
        if (!newTarget) return;

        target = newTarget;

        // Set target to default top-down rotation and start animation
        var originX = Imperium.Settings.Map.RotationLock.Value ? target.rotation.eulerAngles.y : 0;
        cameraTargetRotation = Imperium.Settings.Map.UnlockView.Value
            ? new Vector3(Random.Range(0, 366), 40, 0)
            : new Vector3(originX, 89.9f, 0);
        snapBackAnimationTimer = 1;

        // Use free-look clipping when camera is unlocked
        if (Imperium.Settings.Map.UnlockView.Value) Imperium.Map.SetCameraClipped(false);
    }

    private void InitRenderTexture()
    {
        textureFrame = container.Find("Texture").GetComponent<RawImage>();
        var interactable = textureFrame.gameObject.AddComponent<ImpInteractable>();
        interactable.onDrag += (_, _, delta) => OnDrag(delta);

        var textureRect = textureFrame.GetComponent<RectTransform>();
        var corners = new Vector3[4];
        textureRect.GetWorldCorners(corners);

        var bottomLeft = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
        var topRight = RectTransformUtility.WorldToScreenPoint(null, corners[2]);

        var width = Mathf.RoundToInt(topRight.x - bottomLeft.x);
        var height = Mathf.RoundToInt(topRight.y - bottomLeft.y);
        textureFrame.texture = Imperium.Map.GetRenderTexture(width, height);
    }

    protected override void OnOpen(bool wasOpen)
    {
        Imperium.Map.Camera.enabled = true;
    }

    protected override void OnClose()
    {
        // if (!Imperium.Map.Minimap.IsOpen) Imperium.Map.Camera.enabled = false;

        // Reset camera rotation to match target when closing the UI and rotation lock is enabled
        if (Imperium.Settings.Map.RotationLock.Value && target) mouseOffsetX = 0;
    }

    /// <summary>
    ///     We don't call the base update function here, as the underlaying layer selector doesn't need to be
    ///     opened or closed.
    /// </summary>
    private void Update()
    {
        if (Imperium.Settings.Map.CompassEnabled.Value)
        {
            var rotationY = Imperium.Map.Camera.transform.rotation.eulerAngles.y;
            compass.transform.rotation = Quaternion.Euler(new Vector3(0, 0, rotationY));

            // Counter-rotate to keep the labels upright
            compassNorth.localRotation = Quaternion.Euler(new Vector3(0, 0, -rotationY));
            compassEast.localRotation = Quaternion.Euler(new Vector3(0, 0, -rotationY));
            compassSouth.localRotation = Quaternion.Euler(new Vector3(0, 0, -rotationY));
            compassWest.localRotation = Quaternion.Euler(new Vector3(0, 0, -rotationY));
        }
    }

    private void LateUpdate()
    {
        // Camera sliding animation
        if (snapBackAnimationTimer > 0 && target)
        {
            // Camera rotation
            mouseOffsetX = Mathf.Lerp(
                mouseOffsetX,
                // Compensate for target rotation if rotation lock is enabled
                Imperium.Settings.Map.RotationLock.Value
                    ? cameraTargetRotation.x - target.rotation.eulerAngles.y
                    : cameraTargetRotation.x,
                1 - snapBackAnimationTimer
            );
            mouseOffsetY = Mathf.Lerp(
                mouseOffsetY,
                cameraTargetRotation.y,
                1 - snapBackAnimationTimer
            );

            // View origin translation
            cameraViewOrigin = Vector3.Lerp(
                cameraViewOrigin,
                target.position,
                1 - snapBackAnimationTimer
            );

            snapBackAnimationTimer -= Time.deltaTime;

            // Reset clipping at the end of the animation if auto clipping is on
            // if (Imperium.Settings.Map.AutoClipping.Value
            //     && !Imperium.Settings.Map.UnlockView.Value
            //     && snapBackAnimationTimer < 0.5f
            //     /*&& (PlayerAvatar.instance.isInsideFactory
            //         || PlayerAvatar.instance.isInElevator
            //         || PlayerAvatar.instance.isInHangarShipRoom)*/)
            // {
            //     Imperium.Map.SetCameraClipped(true);
            // }
        }
        else if (target)
        {
            cameraViewOrigin = target.position;

            // Make sure the camera rotation is always fixed when the minimap is open and rotation lock is enabled
            if (Imperium.Settings.Map.RotationLock.Value && Imperium.Map.Minimap.IsOpen) mouseOffsetX = 0;
        }

        // Camera position update
        var direction = new Vector3(0, 0, -30.0f);
        // Add target rotation if rotation lock is activated
        var dragX = target && Imperium.Settings.Map.RotationLock.Value
            ? mouseOffsetX + target.rotation.eulerAngles.y
            : mouseOffsetX;
        var rotation = Quaternion.Euler(mouseOffsetY, dragX, 0);
        Imperium.Map.SetCameraPosition(cameraViewOrigin + rotation * direction, cameraViewOrigin);
    }
}