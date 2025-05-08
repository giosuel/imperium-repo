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
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.Common;

/// <summary>
///     Represents an input field in the Imperium UI, the UI structure should look like this.
///     Parent (RectTransform)
///       - Input (TMP_input)
///         - (Input Stuff)
///       - Title (TMP_Text) [Optional]
///       - Reset (Button) [Optional]
/// </summary>
public abstract class ImpInput
{
    /// <summary>
    ///     Binds an existing input field to an ImpBinding (content type int).
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="valueBinding"></param>
    /// <param name="min">Minimum input value</param>
    /// <param name="max">Maximum input value</param>
    /// <param name="placeholder">A text that is shown when the input field is empty</param>
    /// <param name="negativeIsEmpty">Whether the input field should be cleared on updates with negative value</param>
    /// <param name="updateOnSubmit">Only update the value binding when the input value is submitted</param>
    /// <param name="allowReset">Only update the value binding when the input value is submitted</param>
    /// <param name="theme">The theme the input field will use</param>
    /// <param name="tooltipDefinition">The tooltip definition of the button tooltip.</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static TMP_InputField Bind(
        string path,
        Transform container,
        IBinding<int> valueBinding,
        int min = int.MinValue,
        int max = int.MaxValue,
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

        // If value is below zero and negativeIsEmpty is true, set input field empty
        input.text = valueBinding.Value < 0 && negativeIsEmpty ? "" : valueBinding.Value.ToString();

        input.onValueChanged.AddListener(value =>
        {
            OnIntFieldInput(input, value, min, max);

            if (!updateOnSubmit) UpdateBinding(value);
        });

        if (updateOnSubmit) input.onSubmit.AddListener(UpdateBinding);

        valueBinding.OnUpdate += value =>
        {
            // If value is below zero and negativeIsEmpty is true, set input field empty
            input.text = valueBinding.Value < 0 && negativeIsEmpty ? "" : value.ToString();
        };
        
        // Set placeholder text
        inputParent.Find("Input/Text Area/Placeholder").GetComponent<TMP_Text>().text = placeholder;

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
            BindInteractableBindings(inputParent, input, interactableInvert, interactableBindings);
        }

        if (theme != null)
        {
            OnThemeUpdate(theme.Value, inputParent);
            theme.OnUpdate += value => OnThemeUpdate(value, inputParent);
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
    }

    /// <summary>
    ///     Binds an existing input field to an ImpBinding (content type float).
    /// </summary>
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="valueBinding"></param>
    /// <param name="min">Minimum input value</param>
    /// <param name="max">Maximum input value</param>
    /// <param name="placeholder">A text that is shown when the input field is empty</param>
    /// <param name="negativeIsEmpty">Whether the input field should be cleared on updates with negative value</param>
    /// <param name="updateOnSubmit">Only update the value binding when the input value is submitted</param>
    /// <param name="allowReset">Only update the value binding when the input value is submitted</param>
    /// <param name="theme">The theme the input field will use</param>
    /// <param name="tooltipDefinition">The tooltip definition of the button tooltip.</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static TMP_InputField Bind(
        string path,
        Transform container,
        IBinding<float> valueBinding,
        float min = float.MinValue,
        float max = float.MaxValue,
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
        
        // Set placeholder text
        inputParent.Find("Input/Text Area/Placeholder").GetComponent<TMP_Text>().text = placeholder;

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
            BindInteractableBindings(inputParent, input, interactableInvert, interactableBindings);
        }

        if (theme != null)
        {
            OnThemeUpdate(theme.Value, inputParent);
            theme.OnUpdate += value => OnThemeUpdate(value, inputParent);
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
    /// <param name="path"></param>
    /// <param name="container"></param>
    /// <param name="valueBinding"></param>
    /// <param name="placeholder">A text that is shown when the input field is empty</param>
    /// <param name="updateOnSubmit">Only update the value binding when the input value is submitted</param>
    /// <param name="allowReset">Only update the value binding when the input value is submitted</param>
    /// <param name="theme">The theme the input field will use</param>
    /// <param name="tooltipDefinition">The tooltip definition of the button tooltip.</param>
    /// <param name="interactableInvert">Whether the interactable binding values should be inverted</param>
    /// <param name="interactableBindings">List of boolean bindings that decide if the button is interactable</param>
    internal static TMP_InputField Bind(
        string path,
        Transform container,
        IBinding<string> valueBinding,
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

        // Set binding to default value if input value is empty
        input.onValueChanged.AddListener(value =>
        {
            if (!updateOnSubmit) UpdateBinding(value);
        });

        if (updateOnSubmit) input.onSubmit.AddListener(UpdateBinding);

        valueBinding.OnUpdate += value => input.text = value.ToString();
        
        // Set placeholder text
        inputParent.Find("Input/Text Area/Placeholder").GetComponent<TMP_Text>().text = placeholder;

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
            BindInteractableBindings(inputParent, input, interactableInvert, interactableBindings);
        }

        if (theme != null)
        {
            OnThemeUpdate(theme.Value, inputParent);
            theme.OnUpdate += value => OnThemeUpdate(value, inputParent);
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
        Transform inputParent,
        TMP_InputField input,
        bool interactableInvert = false,
        params IBinding<bool>[] interactableBindings
    )
    {
        var title = inputParent.Find("Title")?.GetComponent<TMP_Text>();

        ToggleInteractable(
            input, title,
            interactableBindings.All(entry => entry.Value),
            interactableInvert
        );

        foreach (var interactableBinding in interactableBindings)
        {
            interactableBinding.OnUpdate += value => ToggleInteractable(
                input, title,
                interactableBindings.All(entry => entry.Value),
                interactableInvert
            );
        }
    }

    private static void ToggleInteractable(
        TMP_InputField input,
        [CanBeNull] TMP_Text title,
        bool isOn, bool inverted = false
    )
    {
        input.interactable = inverted ? !isOn : isOn;

        if (title) ImpUtils.Interface.ToggleTextActive(title, inverted ? !isOn : isOn);
    }
}