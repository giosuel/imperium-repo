#region

using System.Collections.Generic;
using System.Linq;
using Imperium.Core;
using Imperium.Interface.ImperiumUI;
using Imperium.Types;
using Imperium.Util;
using JetBrains.Annotations;
using Librarium;
using Librarium.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.Common;

/// <summary>
///     Represents a dropdown in the Imperium UI, the UI structure should look like this.
///     Parent (RectTransform)
///     - Dropdown (TMP_Dropdown)
///     - Label (TMP_Text)
///     - Placeholder (TMP_Text)
///     - Arrow (Image)
///     - Template (Dropdown Template Stuff)
///     - Title (TMP_Text) [Optional]
///     - Reset (Button) [Optional]
/// </summary>
public static class ImpDropdown
{
    /// <summary>
    ///     Binds an existing dropdown to an binding.
    /// </summary>
    /// <param name="path">The path to the element relative to the container</param>
    /// <param name="container">The parent container of the element</param>
    /// <param name="valueBinding">The binding the element should be bound to</param>
    /// <param name="options">The list of dropdown options the user can pick from</param>
    /// <param name="label">The label that is shown next to the dropdown</param>
    /// <param name="placeholder">A text that is shown when no dropdown value is selected</param>
    /// <param name="allowReset">Whether to show a reset button next to the slider</param>
    /// <param name="playClickSound">Whether to play a click sound when the dropdown value is changed</param>
    /// <param name="theme">The theme the dropdown will use</param>
    /// <param name="tooltipDefinition">The definition of the tooltip that is shown when the cursor hovers over the element</param>
    /// <param name="parentWindow">The window that the element is placed in. Setting this allows the highlighter to highlight this element</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the dropdown is interactable</param>
    /// <param name="interactableInvert">Whether the inte/ractable binding values should be inverted</param>
    /// <returns></returns>
    internal static TMP_Dropdown Bind(
        string path,
        Transform container,
        IBinding<int> valueBinding,
        IEnumerable<string> options,
        string label = "",
        string placeholder = "",
        bool allowReset = true,
        bool playClickSound = true,
        IBinding<ImpTheme> theme = null,
        TooltipDefinition tooltipDefinition = null,
        ImperiumWindow parentWindow = null,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        var dropdownParent = container.Find(path)?.GetComponent<RectTransform>();
        if (!dropdownParent)
        {
            Imperium.IO.LogInfo($"[UI] Failed to bind dropdown element '{Debugging.GetTransformPath(container)}/{path}'");
            return null;
        }

        var dropdown = dropdownParent.Find("Dropdown").GetComponent<TMP_Dropdown>();
        dropdown.options = options.Select(option => new TMP_Dropdown.OptionData(option)).ToList();
        dropdown.value = valueBinding.Value;

        var dropdownLabel = dropdownParent.Find("Dropdown/Label")?.GetComponent<TMP_Text>();
        var dropdownArrow = dropdownParent.Find("Dropdown/Arrow")?.GetComponent<Image>();

        // Set label test if label element exists
        var labelText = dropdownParent.Find("Text")?.GetComponent<TMP_Text>();
        if (labelText && !string.IsNullOrEmpty(label)) labelText.text = label;

        dropdown.onValueChanged.AddListener(value =>
        {
            if (value == valueBinding.Value) return;

            valueBinding.Set(value);
        });

        valueBinding.OnUpdate += value => dropdown.value = value;

        // This has to be in local, so we can manually skip the click sound by disabling send local
        valueBinding.OnTriggerSecondary += () =>
        {
            if (Imperium.Settings.Preferences.PlaySounds.Value && playClickSound) GameUtils.PlayClip(ImpAssets.ButtonClick);
        };

        // Set placeholder text if placeholder element exists
        if (!string.IsNullOrEmpty(placeholder))
        {
            var placeholderText = dropdownParent.Find("Dropdown/Placeholder")?.GetComponent<TMP_Text>();
            if (placeholderText) placeholderText.text = placeholder;
        }

        // Bind reset button if available
        var resetButton = dropdownParent.Find("Reset");
        if (resetButton)
        {
            if (allowReset)
            {
                ImpButton.Bind(
                    "Reset",
                    dropdownParent,
                    () => valueBinding.Reset(),
                    theme: theme,
                    interactableInvert: interactableInvert,
                    interactableBindings: interactableBindings
                );
            }
            else
            {
                resetButton.gameObject.SetActive(false);
            }
        }

        // Add tooltip to parent element if tooltip is provided
        if (tooltipDefinition != null)
        {
            ImpUtils.Interface.AddTooltip(tooltipDefinition, labelText?.transform ?? dropdownParent);
        }

        // Bind all interactable bindings if any were provided
        if (interactableBindings.Length > 0)
        {
            ToggleInteractable(
                dropdown, labelText, dropdownLabel, dropdownArrow,
                interactableBindings.All(entry => entry == null || entry.Value),
                interactableInvert
            );

            foreach (var interactableBinding in interactableBindings)
            {
                if (interactableBinding == null) continue;

                interactableBinding.OnTrigger += () => ToggleInteractable(
                    dropdown, labelText, dropdownLabel, dropdownArrow,
                    interactableBindings.All(entry => entry == null || entry.Value),
                    interactableInvert
                );
            }
        }

        // Register element in parent if parent was provided
        if (parentWindow) parentWindow.RegisterElement(path, dropdownParent);

        if (theme != null)
        {
            OnThemeUpdate(theme.Value, dropdownParent);
            theme.OnUpdate += value =>
            {
                OnThemeUpdate(value, dropdownParent);

                // Fix interactability after theme update
                ToggleInteractable(
                    dropdown, labelText, dropdownLabel, dropdownArrow,
                    interactableBindings.All(entry => entry == null || entry.Value),
                    interactableInvert
                );
            };
        }

        return dropdown;
    }

    private static void OnThemeUpdate(ImpTheme themeUpdate, Transform container)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container,
            new StyleOverride("Dropdown", Variant.FOREGROUND),
            new StyleOverride("Dropdown/Arrow", Variant.FOREGROUND),
            new StyleOverride("Dropdown/Template", Variant.DARKER),
            new StyleOverride("Dropdown/Template/Viewport/Content/Item/Background", Variant.DARKER),
            new StyleOverride("Dropdown/Template/Scrollbar", Variant.DARKEST),
            new StyleOverride("Dropdown/Template/Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );
    }

    private static void ToggleInteractable(
        TMP_Dropdown dropdown,
        [CanBeNull] TMP_Text title,
        [CanBeNull] TMP_Text label,
        [CanBeNull] Image arrow,
        bool isOn, bool inverted = false
    )
    {
        dropdown.interactable = inverted ? !isOn : isOn;

        if (title) ImpUtils.Interface.ToggleTextActive(title, inverted ? !isOn : isOn);
        if (label) ImpUtils.Interface.ToggleTextActive(label, inverted ? !isOn : isOn);
        if (arrow) ImpUtils.Interface.ToggleImageActive(arrow, inverted ? !isOn : isOn);
    }
}