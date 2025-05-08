using System;
using System.Collections.Generic;
using Librarium.Binding;

namespace Imperium.API.Types.Portals;

public record ImpPortalElement
{
    internal ImpPortalTooltip elementTooltip { get; set; }
    internal ImpBinaryBinding interactableBinding { get; set; }

    /// <summary>
    /// Sets the tooltip displayed when the cursor is hovering over the element.
    /// </summary>
    public ImpPortalElement SetTooltip(ImpPortalTooltip tooltip)
    {
        elementTooltip = tooltip;

        return this;
    }

    /// <summary>
    /// Sets a binding for the element that defines when the element is interactable and when not.
    /// </summary>
    public ImpPortalElement SetInteractableBinding(ImpBinaryBinding binding)
    {
        interactableBinding = binding;

        return this;
    }
}

/// <summary>
/// A tooltip that is shown when the cursor is hovering over an element.
/// </summary>
/// <param name="title">The title of the tooltip</param>
/// <param name="description">The descriptive text below the title</param>
public record ImpPortalTooltip(string title, string description);

/// <summary>
/// A simple button element with an onClick callback.
/// </summary>
/// <param name="title">The text that is displayed inside the button</param>
/// <param name="onClick">A callback that is called when the button is pressed</param>
public record ImpPortalButton(
    string title,
    Action onClick
) : ImpPortalElement;

/// <summary>
/// A text field that allows any characters.
/// </summary>
/// <param name="title">The text that is displayed above the input field</param>
/// <param name="valueBinding">The binding that contains the input field's current value</param>
/// <param name="placeholder">A text that is displayed when the input field's value is empty</param>
/// <param name="allowReset">Whether the element should contain a reset button that resets its value binding to the default value</param>
/// <param name="updateOnSubmit">Whether to only update the value binding when the input field is submitted</param>
public record ImpPortalTextField(
    string title,
    IBinding<string> valueBinding,
    string placeholder = "",
    bool allowReset = true,
    bool updateOnSubmit = false
) : ImpPortalElement;

/// <summary>
/// A text field that allows the input of numbers.
/// </summary>
/// <param name="title">The text that is displayed above the input field</param>
/// <param name="valueBinding">The binding that contains the input field's current value</param>
/// <param name="minValue">The smallest integer value that the input field allows (Default: int.MinValue)</param>
/// <param name="maxValue">The largest integer value that the input field allows (Default int.MaxValue)</param>
/// <param name="placeholder">A text that is displayed when the input field's value is empty</param>
/// <param name="allowReset">Whether the element should contain a reset button that resets its value binding to the default value</param>
/// <param name="negativeIsEmpty">If set to true, any negative value will result in the input field being cleared</param>
/// <param name="updateOnSubmit">Whether to only update the value binding when the input field is submitted</param>
public record ImpPortalNumberField(
    string title,
    IBinding<int> valueBinding,
    int minValue = int.MinValue,
    int maxValue = int.MaxValue,
    string placeholder = "",
    bool allowReset = true,
    bool negativeIsEmpty = false,
    bool updateOnSubmit = false
) : ImpPortalElement;

/// <summary>
/// A text field that allows the input of decimal numbers.
/// </summary>
/// <param name="title">The text that is displayed above the input field</param>
/// <param name="valueBinding">The binding that contains the input field's current value</param>
/// <param name="minValue">The smallest decimal value that the input field allows (Default: float.MinValue)</param>
/// <param name="maxValue">The largest decimal value that the input field allows (Default float.MaxValue)</param>
/// <param name="placeholder">A text that is displayed when the input field's value is empty</param>
/// <param name="allowReset">Whether the element should contain a reset button that resets its value binding to the default value</param>
/// <param name="negativeIsEmpty">If set to true, any negative value will result in the input field being cleared</param>
/// <param name="updateOnSubmit">Whether to only update the value binding when the input field is submitted</param>
public record ImpPortalDecimalField(
    string title,
    IBinding<float> valueBinding,
    float minValue = float.MinValue,
    float maxValue = float.MaxValue,
    string placeholder = "",
    bool allowReset = true,
    bool negativeIsEmpty = false,
    bool updateOnSubmit = false
) : ImpPortalElement;

/// <summary>
/// A dropdown element that lets the user pick one of multiple options.
/// 
/// If the value binding contains a negative value, the dropdown will be empty.
/// </summary>
/// <param name="title">The text that is displayed above the input field</param>
/// <param name="valueBinding">The binding that contains the dropdown's currently selected value's index</param>
/// <param name="options">A list of options the user can pick from</param>
/// <param name="placeholder">A text that is displayed when the dropdown value is empty</param>
/// <param name="allowReset">Whether the element should contain a reset button that resets its value binding to the default value</param>
public record ImpPortalDropdown(
    string title,
    IBinding<int> valueBinding,
    List<string> options,
    string placeholder = "",
    bool allowReset = true
) : ImpPortalElement;

/// <summary>
/// A toggle element that lets a user toggle a boolean on or off.
/// </summary>
/// <param name="title">The text that is displayed above the input field</param>
/// <param name="valueBinding">The binding that contains the toggle's current state</param>
/// <param name="allowReset">Whether the element should contain a reset button that resets its value binding to the default value</param>
public record ImpPortalToggle(
    string title,
    IBinding<bool> valueBinding,
    bool allowReset = true
) : ImpPortalElement;

/// <summary>
/// 
/// </summary>
/// <param name="title">The text that is displayed next to the the slider</param>
/// <param name="valueBinding">The binding that contains the toggle's current state</param>
public record ImpPortalSlider(
    string title,
    IBinding<bool> valueBinding,
    bool allowReset = true
) : ImpPortalElement;