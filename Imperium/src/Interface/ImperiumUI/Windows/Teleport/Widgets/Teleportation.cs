using System;
using System.Collections;
using System.Collections.Generic;
using Imperium.Extensions;
using Imperium.Interface.Common;
using Imperium.Types;
using Librarium.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Imperium.Interface.ImperiumUI.Windows.Teleport.Widgets;

public class Teleportation : ImpWidget
{
    private Button tpMainEntrance;
    private Button tpShip;
    private Button tpApparatus;

    private Transform extractionPointContainer;
    private GameObject extractionPointTemplate;
    private TMP_Text extractionPointPlaceholder;
    private readonly List<Button> extractionPoints = [];

    private ImpBinding<float> coordinateX;
    private ImpBinding<float> coordinateY;
    private ImpBinding<float> coordinateZ;

    protected override void InitWidget()
    {
        tpMainEntrance = ImpButton.Bind(
            "Presets/MainEntrance", transform,
            () => TeleportTo(Imperium.PlayerManager.MainEntranceTPAnchor.Value),
            theme
        );
        tpShip = ImpButton.Bind(
            "Presets/Ship", transform,
            () => TeleportTo(Imperium.PlayerManager.ShipTPAnchor.Value),
            theme
        );
        tpApparatus = ImpButton.Bind(
            "Presets/Apparatus", transform,
            () => TeleportTo(Imperium.PlayerManager.ApparatusTPAnchor.Value),
            theme
        );
        ImpButton.Bind(
            "Presets/Freecam", transform,
            () => TeleportTo(Imperium.Freecam.transform.position),
            theme
        );

        // We need to set the teleport function as sync callback as the game might teleport the player to different
        // coordinates due to OOB restrictions. That way, the input field would be out of sync with the actual position,
        // so we have to re-set the coords without invoking another teleport that would lead to a stack overflow.
        coordinateX = new ImpBinding<float>(onUpdateSecondary: _ => TeleportToCoords());
        coordinateY = new ImpBinding<float>(onUpdateSecondary: _ => TeleportToCoords());
        coordinateZ = new ImpBinding<float>(onUpdateSecondary: _ => TeleportToCoords());

        ImpInput.Bind("Coords/CoordsX", transform, coordinateX, theme);
        ImpInput.Bind("Coords/CoordsY", transform, coordinateY, theme);
        ImpInput.Bind("Coords/CoordsZ", transform, coordinateZ, theme);

        ImpButton.Bind("Buttons/Interactive", transform, OnInteractive, theme);

        Imperium.InputBindings.BaseMap.Teleport.performed += OnInteractiveTeleport;

        Imperium.IsArenaLoaded.onTrigger += OnOpen;

        InitExtractionPoints();
    }

    protected override void OnOpen()
    {
        tpShip.interactable = Imperium.PlayerManager.ShipTPAnchor.Value != null;
        tpMainEntrance.interactable = Imperium.PlayerManager.MainEntranceTPAnchor.Value != null
                                      && Imperium.IsArenaLoaded.Value;
        tpApparatus.interactable = Imperium.PlayerManager.ApparatusTPAnchor.Value != null
                                   && Imperium.IsArenaLoaded.Value;

        var position = PlayerAvatar.instance.transform.position;
        coordinateX.Set(MathF.Round(position.x, 2), invokeSecondary: false);
        coordinateY.Set(MathF.Round(position.y, 2), invokeSecondary: false);
        coordinateZ.Set(MathF.Round(position.z, 2), invokeSecondary: false);

        InitExtractionPoints();
    }

    private static void OnInteractive()
    {
        Imperium.Freecam.IsFreecamEnabled.Set(false);
        Imperium.ImpPositionIndicator.Activate(Imperium.PlayerManager.TeleportLocalPlayer);
    }

    private void TeleportTo(Vector3? anchor)
    {
        if (anchor == null) return;
        Imperium.PlayerManager.TeleportLocalPlayer(anchor.Value);
        CloseParent();
    }

    private void TeleportToCoords() => StartCoroutine(teleportToCoordsAndUpdate());

    private IEnumerator teleportToCoordsAndUpdate()
    {
        Imperium.PlayerManager.TeleportLocalPlayer(new Vector3(
            coordinateX.Value,
            coordinateY.Value,
            coordinateZ.Value
        ));

        /*
         * Account for waiting for approximate round-trip to server before updating coords.
         *
         * This is for the case the game restricts the player to teleport to the desired coords (e.g. OOB)
         */
        yield return new WaitForSeconds(0.2f);

        coordinateX.Set(MathF.Round(PlayerAvatar.instance.transform.position.x, 2), invokeSecondary: false);
        coordinateY.Set(MathF.Round(PlayerAvatar.instance.transform.position.y, 2), invokeSecondary: false);
        coordinateZ.Set(MathF.Round(PlayerAvatar.instance.transform.position.z, 2), invokeSecondary: false);
    }

    private void InitExtractionPoints()
    {
        extractionPointContainer = transform.Find("ExtractionPoints/ScrollView/Viewport/Content");
        extractionPointPlaceholder = transform.Find("ExtractionPoints/Placeholder").GetComponent<TMP_Text>();
        extractionPointTemplate = extractionPointContainer.Find("Template").gameObject;
        extractionPointTemplate.gameObject.SetActive(false);

        Imperium.IsArenaLoaded.onTrigger += SetExtractionPoints;
        SetExtractionPoints();
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdated)
    {
        ImpThemeManager.Style(
            themeUpdated,
            transform,
            new StyleOverride("ExtractionPoints", Variant.DARKER)
        );

        foreach (var extractionPoint in extractionPoints)
        {
            ImpThemeManager.Style(
                themeUpdated,
                extractionPoint.transform,
                new StyleOverride("", Variant.DARKER)
            );
        }
    }

    private void SetExtractionPoints()
    {
        foreach (var point in extractionPoints) Destroy(point.gameObject);
        extractionPoints.Clear();

        var pointCounter = 0;
        foreach (var obj in FindObjectsOfType<ExtractionPoint>())
        {
            var pointButtonObj = Instantiate(extractionPointTemplate, extractionPointContainer);
            var pointText = pointButtonObj.transform.Find("Text").GetComponent<TMP_Text>();
            pointText.text = $"Extraction Point #{pointCounter + 1}";

            var pointButton = pointButtonObj.transform.GetComponent<Button>();
            pointButton.onClick.AddListener(() => TeleportTo(obj.transform.position));
            ImpThemeManager.Style(
                theme.Value,
                pointButton.transform,
                new StyleOverride("", Variant.DARKER)
            );

            pointButton.gameObject.SetActive(true);

            extractionPoints.Add(pointButton.GetComponent<Button>());
            pointCounter++;
        }

        if (pointCounter > 0)
        {
            extractionPointPlaceholder.gameObject.SetActive(false);
            extractionPointContainer.gameObject.SetActive(true);
        }
        else
        {
            extractionPointPlaceholder.gameObject.SetActive(true);
            extractionPointContainer.gameObject.SetActive(false);
        }
    }

    private static void OnInteractiveTeleport(InputAction.CallbackContext callbackContext)
    {
        // Set origin of indicator to freecam if freecam is enabled
        var origin = Imperium.Freecam.IsFreecamEnabled.Value ? Imperium.Freecam.transform : null;

        if (Imperium.ImpPositionIndicator.IsActive)
        {
            Imperium.ImpPositionIndicator.Deactivate();
        }
        else
        {
            Imperium.ImpPositionIndicator.Activate(Imperium.PlayerManager.TeleportLocalPlayer, origin);
        }
    }
}