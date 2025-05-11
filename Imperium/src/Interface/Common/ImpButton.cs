#region

using System;
using System.Linq;
using Imperium.Core;
using Imperium.Types;
using Imperium.Util;
using JetBrains.Annotations;
using Librarium;
using Librarium.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static Imperium.Util.ImpUtils;

#endregion

namespace Imperium.Interface.Common;

public abstract class ImpButton
{
    /// <summary>
    ///     Binds a unity button with an onclick listener and interactiveBindings
    /// </summary>
    /// <param name="path">The path to the element relative to the container</param>
    /// <param name="container">The parent container of the element</param>
    /// <param name="onClick">The callback that will be called when the button is pressed</param>
    /// <param name="label">The text that is shown within the button</param>
    /// <param name="isIconButton">Whether the button represents an icon button (Used for theming)</param>
    /// <param name="playClickSound">Whether to play a click sound when the button is clicked</param>
    /// <param name="theme">The theme the button will use</param>
    /// <param name="tooltipDefinition">The definition of the tooltip that is shown when the cursor hovers over the element</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static Button Bind(
        string path,
        Transform container,
        Action onClick,
        string label = "",
        bool isIconButton = false,
        bool playClickSound = true,
        IBinding<ImpTheme> theme = null,
        TooltipDefinition tooltipDefinition = null,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        var buttonParent = container.Find(path);
        if (!buttonParent || !buttonParent.TryGetComponent<Button>(out var button))
        {
            Imperium.IO.LogInfo($"[UI] Failed to bind button '{Debugging.GetTransformPath(container)}/{path}'");
            return null;
        }

        button.onClick.AddListener(() =>
        {
            onClick();

            if (Imperium.Settings.Preferences.PlaySounds.Value && playClickSound)
            {
                MenuManager.instance.MenuEffectClick(MenuManager.MenuClickEffectType.Tick, null, 1f, 1f, soundOnly: true);
            }
        });

        // Set label test if label element exists
        var labelText = buttonParent.Find("Text")?.GetComponent<TMP_Text>();
        if (labelText && !string.IsNullOrEmpty(label)) labelText.text = label;

        var icon = buttonParent.Find("Icon")?.GetComponent<Image>();

        // Bind all interactable bindings if any were provided
        if (interactableBindings.Length > 0)
        {
            BindInteractableBindings(buttonParent, button, icon, labelText, interactableInvert, interactableBindings);
        }

        // Add tooltip to parent element if tooltip is provided
        if (tooltipDefinition != null) ImpUtils.Interface.AddTooltip(tooltipDefinition, buttonParent);

        if (theme != null)
        {
            OnThemeUpdate(theme.Value, buttonParent, isIconButton);
            theme.OnUpdate += value =>
            {
                OnThemeUpdate(value, buttonParent, isIconButton);

                // Fix interactability after theme update
                ToggleInteractable(
                    button, icon, labelText,
                    interactableBindings.All(entry => entry == null || entry.Value),
                    interactableInvert
                );
            };
        }

        return button;
    }

    /// <summary>
    ///     Binds a unity button with an onclick listener and interactiveBindings
    ///     This version is meant for arrow buttons that collapse an area
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="collapseArea"></param>
    /// <param name="stateBinding"></param>
    /// <param name="theme">The theme the button will use</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="updateFunction">Optional update function that is executed when the button is pressed</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static void CreateCollapse(
        string path,
        Transform container,
        Transform collapseArea = null,
        IBinding<bool> stateBinding = null,
        IBinding<ImpTheme> theme = null,
        bool interactableInvert = false,
        Action updateFunction = null,
        params IBinding<bool>[] interactableBindings
    )
    {
        var buttonObject = container.Find(path);
        var button = buttonObject.GetComponent<Button>();
        button.onClick.AddListener(() =>
        {
            stateBinding?.Set(!stateBinding.Value);
            if (collapseArea) collapseArea.gameObject.SetActive(!collapseArea.gameObject.activeSelf);
            button.transform.Rotate(0, 0, 180);
            updateFunction?.Invoke();

            if (Imperium.Settings.Preferences.PlaySounds.Value) GameUtils.PlayClip(ImpAssets.ButtonClick);
        });

        if (stateBinding != null && collapseArea)
        {
            stateBinding.OnUpdate += isOn =>
            {
                collapseArea.gameObject.SetActive(isOn);
                button.transform.rotation = Quaternion.Euler(
                    button.transform.rotation.x,
                    button.transform.rotation.y,
                    isOn ? 180 : 0
                );
            };
        }

        if (interactableBindings.Length > 0)
        {
            ToggleInteractable(
                button,
                null, null,
                interactableBindings.All(entry => entry == null || entry.Value),
                interactableInvert
            );

            foreach (var interactableBinding in interactableBindings)
            {
                if (interactableBinding == null) continue;

                interactableBinding.OnTrigger += () => ToggleInteractable(
                    button,
                    null, null,
                    interactableBindings.All(entry => entry == null || entry.Value),
                    interactableInvert
                );
            }
        }

        if (theme != null)
        {
            theme.OnUpdate += value => OnThemeUpdate(value, buttonObject, true);
            OnThemeUpdate(theme.Value, buttonObject, true);
        }
    }

    private static void BindInteractableBindings(
        Transform buttonParent,
        Button button,
        [CanBeNull] Image icon = null,
        [CanBeNull] TMP_Text labelText = null,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        ToggleInteractable(
            button, icon, labelText,
            interactableBindings.All(entry => entry == null || entry.Value),
            interactableInvert
        );

        foreach (var interactableBinding in interactableBindings)
        {
            if (interactableBinding == null) continue;

            interactableBinding.OnTrigger += () => ToggleInteractable(
                button, icon, labelText,
                interactableBindings.All(entry => entry == null || entry.Value),
                interactableInvert
            );
        }
    }

    private static void ToggleInteractable(
        Button button,
        [CanBeNull] Image icon,
        [CanBeNull] TMP_Text text,
        bool isOn,
        bool inverted
    )
    {
        button.interactable = inverted ? !isOn : isOn;

        if (icon) ImpUtils.Interface.ToggleImageActive(icon, inverted ? !isOn : isOn);
        if (text) ImpUtils.Interface.ToggleTextActive(text, inverted ? !isOn : isOn);
    }

    private static void OnThemeUpdate(ImpTheme theme, Transform container, bool isIconButton)
    {
        if (isIconButton)
        {
            ImpThemeManager.Style(
                theme,
                container,
                new StyleOverride("", Variant.LIGHTER),
                new StyleOverride("Icon", Variant.LIGHTER)
            );
        }
        else
        {
            ImpThemeManager.Style(
                theme,
                container,
                new StyleOverride("", Variant.DARKER),
                new StyleOverride("Icon", Variant.DARKER)
            );
        }
    }
}