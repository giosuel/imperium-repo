using System.Collections.Generic;
using System.Linq;
using Imperium.Types;
using Imperium.Util;
using JetBrains.Annotations;
using Librarium;
using Librarium.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Imperium.Interface.Common;

/// <summary>
///     Represents a dropdown in the Imperium UI, the UI structure should look like this.
///     Parent (RectTransform)
///       - Dropdown (TMP_Dropdown)
///         - Label (TMP_Text)
///         - Placeholder (TMP_Text)
///         - Arrow (Image)
///         - Template (Dropdown Template Stuff)
///       - Title (TMP_Text) [Optional]
///       - Reset (Button) [Optional]
/// </summary>
public static class ImpDropdown
{
    internal static TMP_Dropdown Bind(
        string path,
        Transform container,
        IBinding<int> valueBinding,
        IEnumerable<string> options,
        string placeholder = "",
        bool playClickSound = true,
        bool allowReset = true,
        IBinding<ImpTheme> theme = null,
        TooltipDefinition tooltipDefinition = null,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        var dropdownParent = container.Find(path);
        if (!dropdownParent)
        {
            Imperium.IO.LogInfo($"[UI] Failed to bind dropdown element '{Debugging.GetTransformPath(container)}/{path}'");
            return null;
        }

        var dropdown = dropdownParent.Find("Dropdown").GetComponent<TMP_Dropdown>();
        dropdown.options = options.Select(option => new TMP_Dropdown.OptionData(option)).ToList();
        dropdown.value = valueBinding.Value;

        dropdown.onValueChanged.AddListener(value =>
        {
            if (value == valueBinding.Value) return;

            valueBinding.Set(value);
        });

        valueBinding.OnUpdate += value => dropdown.value = value;
        
        // Set placeholder text
        dropdownParent.Find("Dropdown/Placeholder").GetComponent<TMP_Text>().text = placeholder;

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
        if (tooltipDefinition != null) ImpUtils.Interface.AddTooltip(tooltipDefinition, dropdownParent);

        // Bind all interactable bindings if any were provided
        if (interactableBindings.Length > 0)
        {
            var title = dropdownParent.Find("Title")?.GetComponent<TMP_Text>();
            var label = dropdownParent.Find("Dropdown/Label")?.GetComponent<TMP_Text>();
            var arrow = dropdownParent.Find("Dropdown/Arrow")?.GetComponent<Image>();

            ToggleInteractable(
                dropdown, title, label, arrow,
                interactableBindings.All(entry => entry.Value),
                interactableInvert
            );

            foreach (var interactableBinding in interactableBindings)
            {
                interactableBinding.OnUpdate += value => ToggleInteractable(
                    dropdown, title, label, arrow,
                    interactableBindings.All(entry => entry.Value),
                    interactableInvert
                );
            }
        }

        if (theme != null)
        {
            OnThemeUpdate(theme.Value, dropdownParent);
            theme.OnUpdate += value => OnThemeUpdate(value, dropdownParent);
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