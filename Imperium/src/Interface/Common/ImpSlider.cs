#region

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Imperium.Core;
using Imperium.Types;
using Imperium.Util;
using Librarium;
using Librarium.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.Common;

public class ImpSlider : MonoBehaviour
{
    private Slider slider;
    private TMP_Text handleText;

    private string indicatorUnit;
    private Func<float, string> indicatorFormatter;

    private float debounceTime;
    private Coroutine debounceCoroutine;

    /// <summary>
    ///     Represents a slider in the Imperium UI, the UI structure should look like this.
    ///     Parent (RectTransform)
    ///       - Label (TMP_Text)
    ///       - Slider (Slider)
    ///         - (Slider Stuff)
    ///       - MinValue (TMP_Text) [Optional]
    ///       - MaxValue (TMP_Text) [Optional]
    ///       - Reset (Button) [Optional]
    /// </summary>
    /// <param name="path">The path to the UI element relative to the parent.</param>
    /// <param name="container">The parent object of the UI elmeent.</param>
    /// <param name="valueBinding">The binding that the value of the slider will be bound to</param>
    /// <param name="minValue">The slider range's lower bound</param>
    /// <param name="maxValue">The slider range's upper bound</param>
    /// <param name="label">The label that is shown next to the slider</param>
    /// <param name="debounceTime">Debounce time for slider updates, useful when value binding is a network binding</param>
    /// <param name="valueUnit">The displayed unit of the value (e.g. % or units)</param>
    /// <param name="handleFormatter">Formatter for a custom handle text</param>
    /// <param name="useWholeNumbers">Whether the slider should only allow the input of whole values</param>
    /// <param name="negativeIsDefault">Whether the default value should be displayed when the binding value is negative</param>
    /// <param name="allowReset">Whether to show a reset button next to the slider</param>
    /// <param name="playClickSound">Whether to play a click sound when the slider value is changed</param>
    /// <param name="theme">The theme the slider will use</param>
    /// <param name="tooltipDefinition">The definition of the tooltip that is shown when the cursor hovers over the element</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the slider is interactable</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    internal static ImpSlider Bind(
        string path,
        Transform container,
        IBinding<float> valueBinding,
        float minValue,
        float maxValue,
        string label = "",
        float debounceTime = 0f,
        string valueUnit = "",
        Func<float, string> handleFormatter = null,
        bool useWholeNumbers = false,
        bool negativeIsDefault = false,
        bool allowReset = true,
        bool playClickSound = true,
        IBinding<ImpTheme> theme = null,
        TooltipDefinition tooltipDefinition = null,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        var sliderParent = container.Find(path);
        if (!sliderParent)
        {
            Imperium.IO.LogInfo($"[UI] Failed to bind slider '{Debugging.GetTransformPath(container)}/{path}'");
            return null;
        }

        var impSlider = sliderParent.gameObject.AddComponent<ImpSlider>();
        impSlider.debounceTime = debounceTime;
        impSlider.indicatorFormatter = handleFormatter;
        impSlider.indicatorUnit = valueUnit;
        impSlider.slider = sliderParent.Find("Slider").GetComponent<Slider>();
        impSlider.handleText = sliderParent.Find("Slider/SlideArea/Handle/Text").GetComponent<TMP_Text>();

        handleFormatter ??= value => $"{Mathf.RoundToInt(value)}";

        // Set the min and max values
        impSlider.slider.minValue = minValue;
        impSlider.slider.maxValue = maxValue;

        var minValueText = sliderParent.Find("MinValue")?.GetComponent<TMP_Text>();
        if (minValueText) minValueText.text = $"{minValue:0.#}{valueUnit}";

        var maxValueText = sliderParent.Find("MaxValue")?.GetComponent<TMP_Text>();
        if (maxValueText) maxValueText.text = $"{maxValue:0.#}{valueUnit}";

        impSlider.slider.wholeNumbers = useWholeNumbers;

        // Set label test if label element exists
        var labelText = sliderParent.Find("Text")?.GetComponent<TMP_Text>();
        if (labelText && !string.IsNullOrEmpty(label)) labelText.text = label;

        var sliderArea = sliderParent.Find("Slider/SliderArea").GetComponent<Image>();

        SetSliderValue(valueBinding.Value);
        valueBinding.OnUpdate += SetSliderValue;

        // This has to be in local, so we can manually skip the click sound by disabling send local
        valueBinding.OnTriggerSecondary += () =>
        {
            if (Imperium.Settings.Preferences.PlaySounds.Value && playClickSound) GameUtils.PlayClip(ImpAssets.ButtonClick);
        };

        impSlider.slider.onValueChanged.AddListener(value =>
        {
            // Skip the update if the binding value is already set to the current slider value
            if (Mathf.Approximately(value, valueBinding.Value)) return;

            // Fixes weird null pointer error after respawning UI
            // if (!impSlider) return;

            if (debounceTime > 0)
            {
                if (impSlider.debounceCoroutine != null) impSlider.StopCoroutine(impSlider.debounceCoroutine);
                impSlider.debounceCoroutine = impSlider.StartCoroutine(
                    impSlider.DebounceSlider(valueBinding, value, ImpAssets.ButtonClick)
                );
            }
            else
            {
                valueBinding.Set(value);
            }
        });

        // Bind reset button if available
        var resetButtonObj = sliderParent.Find("Reset");
        if (resetButtonObj)
        {
            if (allowReset)
            {
                ImpButton.Bind(
                    "Reset",
                    sliderParent,
                    () => valueBinding.Reset(),
                    theme: theme,
                    interactableInvert: interactableInvert,
                    interactableBindings: interactableBindings
                );
            }
            else
            {
                resetButtonObj.gameObject.SetActive(false);
            }
        }

        // Add tooltip to parent element if tooltip is provided
        if (tooltipDefinition != null) ImpUtils.Interface.AddTooltip(tooltipDefinition, sliderParent);

        // Bind all interactable bindings if any were provided
        if (interactableBindings.Length > 0)
        {
            ToggleInteractable(
                impSlider.slider, sliderArea,
                interactableBindings.All(entry => entry == null || entry.Value),
                interactableInvert
            );

            foreach (var interactableBinding in interactableBindings)
            {
                if (interactableBinding == null) continue;

                interactableBinding.OnUpdate += value =>
                {
                    ToggleInteractable(impSlider.slider, sliderArea, value, interactableInvert);
                };
            }
        }

        if (theme != null)
        {
            OnThemeUpdate(theme.Value, sliderParent);
            theme.OnUpdate += value =>
            {
                OnThemeUpdate(value, sliderParent);

                // Fix interactability after theme update
                ToggleInteractable(
                    impSlider.slider, sliderArea,
                    interactableBindings.All(entry => entry == null || entry.Value),
                    interactableInvert
                );
            };
        }

        return impSlider;

        void SetSliderValue(float value)
        {
            var updatedValue = value < 0 && negativeIsDefault ? valueBinding.DefaultValue : value;

            impSlider.slider.value = updatedValue;
            impSlider.handleText.text = $"{handleFormatter(updatedValue)}{valueUnit}";
        }
    }

    private static void OnThemeUpdate(ImpTheme theme, Transform container)
    {
        ImpThemeManager.Style(
            theme,
            container,
            new StyleOverride("Slider/SliderArea", Variant.DARKER),
            new StyleOverride("Slider/SlideArea/Handle", Variant.FOREGROUND)
        );
    }

    private IEnumerator DebounceSlider(IBinding<float> binding, float value, AudioClip clickAudio)
    {
        yield return new WaitForSeconds(debounceTime);
        binding.Set(value);
        GameUtils.PlayClip(clickAudio);
    }

    private void SetIndicatorText(float value)
    {
        handleText.text = indicatorFormatter != null
            ? indicatorFormatter(value)
            : $"{Mathf.RoundToInt(value)}{indicatorUnit}";
    }

    public void SetValue(float value)
    {
        slider.value = value;
        SetIndicatorText(value);
    }

    private static void ToggleInteractable(Selectable input, Image sliderArea, bool isOn, bool inverted)
    {
        input.interactable = inverted ? !isOn : isOn;
        ImpUtils.Interface.ToggleImageActive(sliderArea, !isOn);
    }
}