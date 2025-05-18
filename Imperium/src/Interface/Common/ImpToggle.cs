#region

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
///     Represents a toggle in the Imperium UI, supports two types of structures
///     Parent (RectTransform)
///     - Background (Image)
///     - Checkmark (Image)
///     - Text (TMP_Text) [Optional]
/// </summary>
public abstract class ImpToggle
{
    /// <summary>
    ///     Binds a unity toggle with an ImpBinding and interactiveBindings
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="valueBinding">Binding that decides on the state of the toggle</param>
    /// <param name="label">The label that is shown next to the input field</param>
    /// <param name="theme">The theme the button will use</param>
    /// <param name="playClickSound">Whether the click sound playes when the button is clicked.</param>
    /// <param name="parentWindow">The window that the element is placed in. Setting this allows the highlighter to highlight this element</param>
    /// <param name="tooltipDefinition">The tooltip definition of the toggle tooltip.</param>
    /// <param name="interactableBindings">List of bindings that decide if the button is interactable</param>
    internal static Toggle Bind(
        string path,
        Transform container,
        IBinding<bool> valueBinding,
        string label = "",
        IBinding<ImpTheme> theme = null,
        bool playClickSound = true,
        ImperiumWindow parentWindow = null,
        TooltipDefinition tooltipDefinition = null,
        params IBinding<bool>[] interactableBindings
    )
    {
        var toggleParent = container.Find(path)?.GetComponent<RectTransform>();
        if (!toggleParent)
        {
            Imperium.IO.LogInfo($"[UI] Failed to bind toggle element '{Debugging.GetTransformPath(container)}/{path}'");
            return null;
        }

        var toggle = toggleParent.GetComponent<Toggle>();
        toggle.isOn = valueBinding.Value;

        // Set label test if label element exists
        var labelText = toggleParent.Find("Text")?.GetComponent<TMP_Text>();
        if (labelText && !string.IsNullOrEmpty(label)) labelText.text = label;

        toggle.onValueChanged.AddListener(value =>
        {
            if (value == valueBinding.Value) return;

            valueBinding.Set(value);

            Imperium.IO.LogInfo($"Toggle changed: {Debugging.GetTransformPath(toggleParent)}");
        });

        valueBinding.OnUpdate += value => toggle.isOn = value;

        /*
         * When using an ImpNetworkBinding, we want to make sure the click sound is only played
         * when the update comes from the local client.
         */
        valueBinding.OnUpdateSecondary += _ =>
        {
            if (Imperium.Settings.Preferences.PlaySounds.Value && playClickSound)
            {
                GameUtils.PlayClip(ImpAssets.ButtonClick);
            }
        };

        if (interactableBindings.Length > 0)
        {
            var checkmark = (toggleParent.Find("Background/Checkmark") ?? toggleParent.Find("Checkmark"))
                ?.GetComponent<Image>();

            ToggleInteractable(
                toggle, checkmark, labelText,
                interactableBindings.All(entry => entry == null || entry.Value)
            );

            foreach (var interactableBinding in interactableBindings)
            {
                if (interactableBinding == null) continue;

                interactableBinding.OnTrigger += () => ToggleInteractable(
                    toggle, checkmark, labelText,
                    interactableBindings.All(entry => entry == null || entry.Value)
                );
            }
        }

        // Add tooltip to parent element if tooltip is provided
        if (tooltipDefinition != null) ImpUtils.Interface.AddTooltip(tooltipDefinition, toggleParent);

        // Register element in parent if parent was provided
        if (parentWindow) parentWindow.RegisterElement(path, toggleParent);

        if (theme != null)
        {
            OnThemeUpdate(theme.Value, toggleParent);
            theme.OnUpdate += value => OnThemeUpdate(value, toggleParent);
        }

        return toggle;
    }

    private static void OnThemeUpdate(ImpTheme theme, Transform container)
    {
        if (!container) return;

        ImpThemeManager.Style(
            theme,
            container,
            new StyleOverride("", Variant.FOREGROUND),
            new StyleOverride("Background", Variant.FOREGROUND),
            new StyleOverride("Checkmark", Variant.LIGHTER),
            new StyleOverride("Background/Checkmark", Variant.LIGHTER)
        );
    }

    private static void ToggleInteractable(
        Toggle toggle,
        [CanBeNull] Image checkmark,
        [CanBeNull] TMP_Text labelText,
        bool isOn
    )
    {
        toggle.interactable = isOn;
        if (checkmark) ImpUtils.Interface.ToggleImageActive(checkmark, isOn);
        if (labelText) ImpUtils.Interface.ToggleTextActive(labelText, isOn);
    }
}