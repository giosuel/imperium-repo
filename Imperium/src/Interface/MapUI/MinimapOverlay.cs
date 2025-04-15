#region

using Imperium.Types;
using Imperium.Util;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.MapUI;

internal class MinimapOverlay : BaseUI
{
    internal Rect CameraRect { get; private set; }
    private TMP_Text locationText;
    private TMP_Text positionText;
    private TMP_Text rotationText;
    private TMP_Text levelText;
    private TMP_Text extractionText;
    private GameObject infoPanel;
    private GameObject locationPanel;
    private Transform mapBorder;
    private Canvas canvas;
    private RawImage textureFrame;

    private GameObject compass;
    private Transform compassNorth;
    private Transform compassEast;
    private Transform compassSouth;
    private Transform compassWest;

    private const float BorderThickness = 2f;

    protected override void InitUI()
    {
        mapBorder = container.Find("MapBorder");
        locationPanel = container.Find("MapBorder/LocationPanel").gameObject;
        locationText = locationPanel.transform.Find("Text").GetComponent<TMP_Text>();
        infoPanel = container.Find("MapBorder/InfoPanel").gameObject;
        positionText = container.Find("MapBorder/InfoPanel/Position").GetComponent<TMP_Text>();
        rotationText = container.Find("MapBorder/InfoPanel/Rotation").GetComponent<TMP_Text>();
        levelText = container.Find("MapBorder/InfoPanel/Level").GetComponent<TMP_Text>();
        extractionText = container.Find("MapBorder/InfoPanel/Extraction").GetComponent<TMP_Text>();

        canvas = GetComponent<Canvas>();
        textureFrame = container.Find("Texture").GetComponent<RawImage>();

        var baseCanvasScale = canvas.scaleFactor;
        Imperium.Settings.Map.MinimapScale.onPrimaryUpdate += value => InitMapScale(baseCanvasScale * value);

        // InitMapScale(baseCanvasScale * Imperium.Settings.Map.MinimapScale.Value);
        InitCompass();

        var mapToolController = FindObjectOfType<MapToolController>(true);
        textureFrame.material = mapToolController.DisplayMaterial;
        textureFrame.texture = mapToolController.DisplayMaterial.mainTexture;
    }

    protected override void OnOpen(bool wasOpen)
    {
        textureFrame.texture = Imperium.Map.Camera.targetTexture;
    }

    private void InitMapScale(float scaleFactor)
    {
        canvas.scaleFactor = scaleFactor;
        //
        // var mapBorderPosition = mapBorder.gameObject.GetComponent<RectTransform>().position;
        // var mapBorderSize = mapBorder.gameObject.GetComponent<RectTransform>().sizeDelta;
        // var mapContainerWidth = mapBorderSize.x * canvas.scaleFactor - BorderThickness * 2;
        // var mapContainerHeight = mapBorderSize.y * canvas.scaleFactor - BorderThickness * 2;
        //
        // CameraRect = new Rect(
        //     (mapBorderPosition.x + BorderThickness) / Screen.width,
        //     (mapBorderPosition.y + BorderThickness) / Screen.height,
        //     mapContainerWidth / Screen.width,
        //     mapContainerHeight / Screen.height
        // );
    }

    protected override void OnThemePrimaryUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container.Find("MapBorder"),
            new StyleOverride("", Variant.DARKER),
            new StyleOverride("Compass", Variant.FOREGROUND),
            new StyleOverride("Compass/Icon", Variant.FOREGROUND),
            new StyleOverride("InfoPanel", Variant.BACKGROUND),
            new StyleOverride("InfoPanel/Border", Variant.DARKER),
            new StyleOverride("LocationPanel", Variant.BACKGROUND),
            new StyleOverride("LocationPanel/Border", Variant.DARKER)
        );
        ImpThemeManager.StyleText(
            themeUpdate,
            container.Find("MapBorder"),
            new StyleOverride("Compass/North", Variant.FOREGROUND),
            new StyleOverride("Compass/East", Variant.FOREGROUND),
            new StyleOverride("Compass/South", Variant.FOREGROUND),
            new StyleOverride("Compass/West", Variant.FOREGROUND),
            new StyleOverride("LocationPanel/Text", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/Position", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/PositionTitle", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/Rotation", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/RotationTitle", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/Location", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/LocationTitle", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/Level", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/LevelTitle", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/Extraction", Variant.FOREGROUND),
            new StyleOverride("InfoPanel/ExtractionTitle", Variant.FOREGROUND)
        );
    }

    private void InitCompass()
    {
        compass = container.Find("MapBorder/Compass").gameObject;
        compass.SetActive(Imperium.Settings.Map.CompassEnabled.Value);
        Imperium.Settings.Map.CompassEnabled.onPrimaryUpdate += compass.SetActive;

        compassNorth = compass.transform.Find("North");
        compassEast = compass.transform.Find("East");
        compassSouth = compass.transform.Find("West");
        compassWest = compass.transform.Find("South");
    }

    private void Update()
    {
        // Automatically open this UI when nothing else is open
        if (!Imperium.Settings.Map.MinimapEnabled.Value
            || Imperium.Interface.IsOpen()
            || Imperium.Freecam.IsFreecamEnabled.Value
            || !Imperium.IsImperiumEnabled
            || Imperium.GameManager.IsGameLoading
            || !Imperium.IsArenaLoaded
           )
        {
            if (IsOpen) Close();
        }
        else
        {
            if (!IsOpen) Open();
        }

        if (!IsOpen) return;

        infoPanel.SetActive(Imperium.Settings.Map.MinimapInfoPanel.Value);
        locationPanel.SetActive(Imperium.Settings.Map.MinimapLocationPanel.Value);

        locationText.SetText(LevelGenerator.Instance.Level.NarrativeName);

        // Only update the panel when it's activated
        if (Imperium.Settings.Map.MinimapInfoPanel.Value)
        {
            var playerPosition = PlayerAvatar.instance.transform.position;
            positionText.text = $"{Formatting.FormatVector(playerPosition, separator: "/", roundDigits: 0)}";

            var playerRotation = PlayerAvatar.instance.localCamera.transform.rotation.eulerAngles;
            rotationText.text =
                $"{Formatting.FormatVector(playerRotation, separator: "/", roundDigits: 0, unit: "\u00b0")}";

            levelText.text = $"{RunManager.instance.levelsCompleted + 1}";
            extractionText.text = $"{RoundDirector.instance.extractionPointsCompleted}/{RoundDirector.instance.extractionPoints}";
        }

        // Only update the compass when it's activated
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
}