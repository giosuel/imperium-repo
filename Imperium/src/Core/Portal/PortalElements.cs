using System;
using System.Collections.Generic;
using HarmonyLib;
using Imperium.API.Types.Portals;
using Librarium.Binding;

namespace Imperium.Core.Portal;

public record ImpPortalElement
{
    internal Tooltip tooltip { get; set; }
    internal ImpBinaryBinding interactableBinding { get; set; }

    public ImpPortalElement SetTooltip(string tooltipTitle, string tooltipDescription = "")
    {
        tooltip = new Tooltip
        {
            Title = tooltipTitle,
            Description = tooltipDescription
        };

        return this;
    }

    public ImpPortalElement SetInteractableBinding(ImpBinaryBinding binding)
    {
        interactableBinding = binding;

        return this;
    }
}

public record ImpPortalButton(
    string title,
    Action onClick
) : ImpPortalElement
{
    internal string title { get; init; } = title;
    internal Action onClick { get; init; } = onClick;
}

public record ImpPortalTextField(
    string title,
    IBinding<string> valueBinding,
    int minValue,
    int maxValue
) : ImpPortalElement
{
    internal string title { get; init; } = title;
    internal IBinding<string> valueBinding { get; init; } = valueBinding;
    internal int minValue { get; init; } = minValue;
    internal int maxValue { get; init; } = maxValue;
}

public record ImpPortalNumberField(
    string title,
    IBinding<int> valueBinding,
    int minValue,
    int maxValue
) : ImpPortalElement
{
    internal string title { get; init; } = title;
    internal IBinding<int> valueBinding { get; init; } = valueBinding;
    internal int minValue { get; init; } = minValue;
    internal int maxValue { get; init; } = maxValue;
}

public record ImpPortalDecimalField(
    string title,
    IBinding<float> valueBinding,
    int minValue,
    int maxValue
) : ImpPortalElement
{
    internal string title { get; init; } = title;
    internal IBinding<float> valueBinding { get; init; } = valueBinding;
    internal int minValue { get; init; } = minValue;
    internal int maxValue { get; init; } = maxValue;
}

public record ImpPortalDropdown(
    string title,
    IBinding<int> valueBinding,
    List<string> options
) : ImpPortalElement
{
    internal string title { get; init; } = title;
    internal IBinding<int> valueBinding { get; init; } = valueBinding;
    internal List<string> options { get; init; } = options;
}

public record ImpPortalToggle(
    string title,
    IBinding<bool> valueBinding
) : ImpPortalElement
{
    internal string title { get; init; } = title;
    internal IBinding<bool> valueBinding { get; init; } = valueBinding;
}