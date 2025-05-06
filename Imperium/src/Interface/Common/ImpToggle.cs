#region

using System.Linq;
using Imperium.Core;
using Imperium.Types;
using Imperium.Util;
using JetBrains.Annotations;
using Librarium;
using Librarium.Binding;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.Common;

/// <summary>
///     Represents a toggle in the Imperium UI, supports two types of structures
///     Toggle (Toggle)
///     - Background (Image)
///     - Checkmark (Image)
///     - Text (TMP_Text)
///     Toggle (Toggle, Image)
///     - Checkmark (Image)
/// </summary>
public abstract class ImpToggle
{
    /// <summary>
    ///     Binds a unity toggle with an ImpBinding and interactiveBindings
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="valueBinding">Binding that decides on the state of the toggle</param>
    /// <param name="theme">The theme the button will use</param>
    /// <param name="playClickSound">Whether the click sound playes when the button is clicked.</param>
    /// <param name="tooltipDefinition">The tooltip definition of the toggle tooltip.</param>
    /// <param name="interactableBindings">List of bindings that decide if the button is interactable</param>
    internal static Toggle Bind(
        string path,
        Transform container,
        IBinding<bool> valueBinding,
        IBinding<ImpTheme> theme = null,
        bool playClickSound = true,
        TooltipDefinition tooltipDefinition = null,
        params ImpBinding<bool>[] interactableBindings
    )
    {
        var toggleParent = container.Find(path);
        if (!toggleParent)
        {
            Imperium.IO.LogInfo($"[UI] Failed to bind toggle element '{Debugging.GetTransformPath(container)}/{path}'");
            return null;
        }

        var toggle = toggleParent.GetComponent<Toggle>();
        toggle.isOn = valueBinding.Value;

        toggle.onValueChanged.AddListener(value =>
        {
            if (value == valueBinding.Value) return;

            valueBinding.Set(value);
        });

        // valueBinding.OnUpdate += value => toggle.isOn = value;

        // var interactable = toggleParent.gameObject.AddComponent<ImpInteractable>();
        // interactable.onClick += () =>
        // {
        //     if (!toggle.interactable) return;
        //     valueBinding.Set(!valueBinding.Value);
        // };
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
            var checkmark = toggleParent.Find("Background/Checkmark")?.GetComponent<Image>()
                            ?? toggleParent.Find("Checkmark").GetComponent<Image>();
            var text = (toggleParent.Find("Text") ?? toggleParent.Find("Text (TMP)"))?.GetComponent<TMP_Text>();

            var isOn = interactableBindings.All(entry => entry.Value);
            ToggleInteractable(toggle, checkmark, text, isOn);

            foreach (var interactableBinding in interactableBindings)
            {
                interactableBinding.OnUpdate += value => ToggleInteractable(toggle, checkmark, text, value);
            }
        }

        // Add tooltip to parent element if tooltip is provided
        if (tooltipDefinition != null) ImpUtils.Interface.AddTooltip(tooltipDefinition, toggleParent);

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
        [CanBeNull] TMP_Text text,
        bool isOn
    )
    {
        toggle.interactable = isOn;
        if (checkmark) ImpUtils.Interface.ToggleImageActive(checkmark, isOn);
        if (text) ImpUtils.Interface.ToggleTextActive(text, isOn);
    }
}