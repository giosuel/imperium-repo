#region

using System.Globalization;
using System.Linq;
using Imperium.Types;
using Imperium.Util;
using JetBrains.Annotations;
using Librarium;
using Librarium.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.Common;

/// <summary>
///     Represents an input field in the Imperium UI, the UI structure should look like this.
///     Parent (RectTransform)
///     - Input (TMP_input)
///     - (Input Stuff)
///     - Label (TMP_Text) [Optional]
///     - Reset (Button) [Optional]
/// </summary>
public abstract class ImpInput
{
    /// <summary>
    ///     Binds an existing input field to a binding (content type int).
    /// </summary>
    /// <param name="path">The path to the element relative to the container</param>
    /// <param name="container">The parent container of the element</param>
    /// <param name="valueBinding">The binding the element should be bound to</param>
    /// <param name="min">Minimum input value</param>
    /// <param name="max">Maximum input value</param>
    /// <param name="label">The label that is shown next to the input field</param>
    /// <param name="placeholder">A text that is shown when the input field is empty</param>
    /// <param name="negativeIsEmpty">Whether the input field should be cleared on updates with negative value</param>
    /// <param name="updateOnSubmit">Only update the value binding when the input value is submitted</param>
    /// <param name="allowReset">Whether to show a reset button next to the input field</param>
    /// <param name="theme">The theme the input field will use</param>
    /// <param name="tooltipDefinition">The definition of the tooltip that is shown when the cursor hovers over the element</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static TMP_InputField Bind(
        string path,
        Transform container,
        IBinding<int> valueBinding,
        int min = int.MinValue,
        int max = int.MaxValue,
        string label = "",
        string placeholder = "",
        bool negativeIsEmpty = false,
        bool updateOnSubmit = false,
        bool allowReset = true,
        IBinding<ImpTheme> theme = null,
        TooltipDefinition tooltipDefinition = null,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        var inputParent = container.Find(path);
        if (!inputParent)
        {
            Imperium.IO.LogInfo($"[UI] Failed to bind input element '{Debugging.GetTransformPath(container)}/{path}'");
            return null;
        }

        var input = inputParent.Find("Input").GetComponent<TMP_InputField>();
        input.contentType = TMP_InputField.ContentType.IntegerNumber;

        SetInputValue(valueBinding.Value);
        valueBinding.OnUpdate += SetInputValue;

        input.onValueChanged.AddListener(value =>
        {
            OnIntFieldInput(input, value, min, max);

            if (!updateOnSubmit) UpdateBinding(value);
        });

        if (updateOnSubmit) input.onSubmit.AddListener(UpdateBinding);

        // Set label test if label element exists
        var labelText = inputParent.Find("Text")?.GetComponent<TMP_Text>();
        if (labelText && !string.IsNullOrEmpty(label)) labelText.text = label;

        // Set placeholder text if placeholder element exists
        TMP_Text placeholderText = null;
        if (!string.IsNullOrEmpty(placeholder))
        {
            placeholderText = inputParent.Find("Input/Text Area/Placeholder")?.GetComponent<TMP_Text>();
            if (placeholderText) placeholderText.text = placeholder;
        }

        // Bind reset button if available
        var resetButton = inputParent.Find("Reset");
        if (resetButton)
        {
            if (allowReset)
            {
                ImpButton.Bind(
                    "Reset",
                    inputParent,
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
        if (tooltipDefinition != null) ImpUtils.Interface.AddTooltip(tooltipDefinition, inputParent);

        // Bind all interactable bindings if any were provided
        if (interactableBindings.Length > 0)
        {
            BindInteractableBindings(input, labelText, placeholderText, interactableInvert, interactableBindings);
        }

        if (theme != null)
        {
            OnThemeUpdate(theme.Value, inputParent);
            theme.OnUpdate += value =>
            {
                OnThemeUpdate(value, inputParent);

                // Fix interactability after theme update
                ToggleInteractable(
                    input, labelText, placeholderText,
                    interactableBindings.All(entry => entry == null || entry.Value),
                    interactableInvert
                );
            };
        }

        return input;

        void UpdateBinding(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                // Reset binding on empty value if is not already default
                if (valueBinding.Value != valueBinding.DefaultValue) valueBinding.Reset();
            }
            else
            {
                var parsed = int.Parse(value);

                // Update binding if binding value is not already equal to input value
                if (parsed != valueBinding.Value) valueBinding.Set(parsed);
            }
        }

        void SetInputValue(int value)
        {
            // If value is below zero and negativeIsEmpty is true, set input field empty
            input.text = value < 0 && negativeIsEmpty ? "" : value.ToString();
        }
    }

    /// <summary>
    ///     Binds an existing input field to an ImpBinding (content type float).
    /// </summary>
    /// <param name="path">The path to the element relative to the container</param>
    /// <param name="container">The parent container of the element</param>
    /// <param name="valueBinding">The binding the element should be bound to</param>
    /// <param name="min">Minimum input value</param>
    /// <param name="max">Maximum input value</param>
    /// <param name="label">The label that is shown next to the input field</param>
    /// <param name="placeholder">A text that is shown when the input field is empty</param>
    /// <param name="negativeIsEmpty">Whether the input field should be cleared on updates with negative value</param>
    /// <param name="updateOnSubmit">Only update the value binding when the input value is submitted</param>
    /// <param name="allowReset">Whether to show a reset button next to the input field</param>
    /// <param name="theme">The theme the input field will use</param>
    /// <param name="tooltipDefinition">The definition of the tooltip that is shown when the cursor hovers over the element</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static TMP_InputField Bind(
        string path,
        Transform container,
        IBinding<float> valueBinding,
        float min = float.MinValue,
        float max = float.MaxValue,
        string label = "",
        string placeholder = "",
        bool negativeIsEmpty = false,
        bool updateOnSubmit = false,
        bool allowReset = false,
        IBinding<ImpTheme> theme = null,
        TooltipDefinition tooltipDefinition = null,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        var inputParent = container.Find(path);
        if (!inputParent)
        {
            Imperium.IO.LogInfo($"[UI] Failed to bind input element '{Debugging.GetTransformPath(container)}/{path}'");
            return null;
        }

        var input = inputParent.Find("Input").GetComponent<TMP_InputField>();
        input.contentType = TMP_InputField.ContentType.DecimalNumber;

        // If value is below zero and negativeIsEmpty is true, set input field empty
        input.text = valueBinding.Value < 0 && negativeIsEmpty
            ? ""
            : valueBinding.Value.ToString(CultureInfo.InvariantCulture);

        input.onValueChanged.AddListener(value =>
        {
            OnFloatFieldInput(input, value, min, max);

            if (!updateOnSubmit) UpdateBinding(value);
        });

        if (updateOnSubmit) input.onSubmit.AddListener(UpdateBinding);

        valueBinding.OnUpdate += value =>
        {
            // If value is below zero and negativeIsEmpty is true, set input field empty
            input.text = valueBinding.Value < 0 && negativeIsEmpty
                ? ""
                : value.ToString(CultureInfo.InvariantCulture);
        };

        // Set label test if label element exists
        var labelText = inputParent.Find("Text")?.GetComponent<TMP_Text>();
        if (labelText && !string.IsNullOrEmpty(label)) labelText.text = label;

        // Set placeholder text if placeholder element exists
        TMP_Text placeholderText = null;
        if (!string.IsNullOrEmpty(placeholder))
        {
            placeholderText = inputParent.Find("Input/Text Area/Placeholder")?.GetComponent<TMP_Text>();
            if (placeholderText) placeholderText.text = placeholder;
        }

        // Bind reset button if available
        var resetButton = inputParent.Find("Reset");
        if (resetButton)
        {
            if (allowReset)
            {
                ImpButton.Bind(
                    "Reset",
                    inputParent,
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
        if (tooltipDefinition != null) ImpUtils.Interface.AddTooltip(tooltipDefinition, inputParent);

        // Bind all interactable bindings if any were provided
        if (interactableBindings.Length > 0)
        {
            BindInteractableBindings(input, labelText, placeholderText, interactableInvert, interactableBindings);
        }

        if (theme != null)
        {
            OnThemeUpdate(theme.Value, inputParent);
            theme.OnUpdate += value =>
            {
                OnThemeUpdate(value, inputParent);

                // Fix interactability after theme update
                ToggleInteractable(
                    input, labelText, placeholderText,
                    interactableBindings.All(entry => entry == null || entry.Value),
                    interactableInvert
                );
            };
        }

        return input;

        void UpdateBinding(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                // Reset binding on empty value if is not already default
                if (!Mathf.Approximately(valueBinding.Value, valueBinding.DefaultValue)) valueBinding.Reset();
            }
            else
            {
                var parsed = float.Parse(value);

                // Update binding if binding value is not already equal to input value
                if (!Mathf.Approximately(parsed, valueBinding.Value)) valueBinding.Set(parsed);
            }
        }
    }

    /// <summary>
    ///     Binds an existing input field to an ImpBinding (content type string).
    /// </summary>
    /// <param name="path">The path to the element relative to the container</param>
    /// <param name="container">The parent container of the element</param>
    /// <param name="valueBinding">The binding the element should be bound to</param>
    /// <param name="label">The label that is shown next to the input field</param>
    /// <param name="placeholder">A text that is shown when the input field is empty</param>
    /// <param name="updateOnSubmit">Only update the value binding when the input value is submitted</param>
    /// <param name="allowReset">Whether to show a reset button next to the input field</param>
    /// <param name="theme">The theme the input field will use</param>
    /// <param name="tooltipDefinition">The definition of the tooltip that is shown when the cursor hovers over the element</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static TMP_InputField Bind(
        string path,
        Transform container,
        IBinding<string> valueBinding,
        string label = "",
        string placeholder = "",
        bool updateOnSubmit = false,
        bool allowReset = false,
        IBinding<ImpTheme> theme = null,
        TooltipDefinition tooltipDefinition = null,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        var inputParent = container.Find(path);
        if (!inputParent)
        {
            Imperium.IO.LogInfo($"[UI] Failed to bind input element '{Debugging.GetTransformPath(container)}/{path}'");
            return null;
        }

        var input = inputParent.Find("Input").GetComponent<TMP_InputField>();
        input.contentType = TMP_InputField.ContentType.Standard;

        input.text = valueBinding.Value;

        input.onValueChanged.AddListener(value =>
        {
            if (!updateOnSubmit) UpdateBinding(value);
        });

        if (updateOnSubmit) input.onSubmit.AddListener(UpdateBinding);

        valueBinding.OnUpdate += value => input.text = value.ToString();

        // Set label test if label element exists
        var labelText = inputParent.Find("Text")?.GetComponent<TMP_Text>();
        if (labelText && !string.IsNullOrEmpty(label)) labelText.text = label;

        // Set placeholder text if placeholder element exists
        TMP_Text placeholderText = null;
        if (!string.IsNullOrEmpty(placeholder))
        {
            placeholderText = inputParent.Find("Input/Text Area/Placeholder")?.GetComponent<TMP_Text>();
            if (placeholderText) placeholderText.text = placeholder;
        }

        // Bind reset button if available
        var resetButton = inputParent.Find("Reset");
        if (resetButton)
        {
            if (allowReset)
            {
                ImpButton.Bind(
                    "Reset",
                    inputParent,
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
        if (tooltipDefinition != null) ImpUtils.Interface.AddTooltip(tooltipDefinition, inputParent);

        // Bind all interactable bindings if any were provided
        if (interactableBindings.Length > 0)
        {
            BindInteractableBindings(input, labelText, placeholderText, interactableInvert, interactableBindings);
        }

        if (theme != null)
        {
            OnThemeUpdate(theme.Value, inputParent);
            theme.OnUpdate += value =>
            {
                OnThemeUpdate(value, inputParent);

                // Fix interactability after theme update
                ToggleInteractable(
                    input, labelText, placeholderText,
                    interactableBindings.All(entry => entry == null || entry.Value),
                    interactableInvert
                );
            };
        }

        return input;

        void UpdateBinding(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                // Reset binding on empty value if is not already default
                if (valueBinding.Value != valueBinding.DefaultValue) valueBinding.Reset();
            }
            else
            {
                // Update binding if binding value is not already equal to input value
                if (value != valueBinding.Value) valueBinding.Set(value);
            }
        }
    }

    private static void OnThemeUpdate(ImpTheme themeUpdate, Transform container)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container,
            new StyleOverride("Input", Variant.FOREGROUND)
        );
    }

    private static void OnIntFieldInput(
        TMP_InputField field,
        string text,
        int min = int.MinValue,
        int max = int.MaxValue
    )
    {
        if (string.IsNullOrEmpty(text) || !int.TryParse(text, out var value)) return;

        if (value > max)
        {
            field.text = max.ToString();
        }
        else if (value < min)
        {
            field.text = min.ToString();
        }
    }

    private static void OnFloatFieldInput(
        TMP_InputField field,
        string text,
        float min = float.MinValue,
        float max = float.MaxValue
    )
    {
        if (string.IsNullOrEmpty(text) || !float.TryParse(text, out var value)) return;

        if (value > max)
        {
            field.text = max.ToString(CultureInfo.InvariantCulture);
        }
        else if (value < min)
        {
            field.text = min.ToString(CultureInfo.InvariantCulture);
        }
    }

    private static void BindInteractableBindings(
        TMP_InputField input,
        [CanBeNull] TMP_Text labelText,
        [CanBeNull] TMP_Text placeholderText,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        ToggleInteractable(
            input, labelText, placeholderText,
            interactableBindings.All(entry => entry == null || entry.Value),
            interactableInvert
        );

        foreach (var interactableBinding in interactableBindings)
        {
            if (interactableBinding == null) continue;

            interactableBinding.OnTrigger += () => ToggleInteractable(
                input, labelText, placeholderText,
                interactableBindings.All(entry => entry == null || entry.Value),
                interactableInvert
            );
        }
    }

    private static void ToggleInteractable(
        TMP_InputField input,
        [CanBeNull] TMP_Text labelText,
        [CanBeNull] TMP_Text placeholderText,
        bool isOn, bool inverted = false
    )
    {
        input.interactable = inverted ? !isOn : isOn;

        if (labelText) ImpUtils.Interface.ToggleTextActive(labelText, inverted ? !isOn : isOn);
        if (placeholderText) ImpUtils.Interface.ToggleTextActive(placeholderText, inverted ? !isOn : isOn);
    }
}