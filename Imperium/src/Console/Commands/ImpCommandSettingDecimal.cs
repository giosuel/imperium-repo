using System.Linq;
using Imperium.Core;
using Imperium.Interface.ImperiumUI;
using Imperium.Util;
using Librarium.Binding;
using UnityEngine;

namespace Imperium.Console.Commands;

/// <summary>
/// Console command to change the value of a float binding.
/// </summary>
/// <param name="name">The display name of the command</param>
/// <param name="binding">The binding that will be changed</param>
/// <param name="minValue">Optional min value that is allowed in the binding</param>
/// <param name="maxValue">Optional max value that is allowed in the binding</param>
/// <param name="valueUnit">The displayed unit of the value (e.g. % or units)</param>
/// <param name="customIcon">A custom icon that is displayed next to the command</param>
internal class ImpCommandSettingDecimal(
    string name,
    IBinding<float> binding,
    float? minValue,
    float? maxValue,
    string valueUnit = null,
    string interfacePath = null,
    Sprite customIcon = null,
    bool enabledBindingInverted = false,
    IBinding<bool> enabledBinding = null
) : ImpCommand(name, customIcon, enabledBindingInverted, enabledBinding)
{
    internal override string DisplayType => "Setting";
    internal override Sprite Icon => customIcon ?? ImpAssets.IconCommandSetting;

    internal override bool Execute(ConsoleQuery query)
    {
        if (query.Keyword == QueryKeyword.Reset ||
            query.Args.Length > 0 && ImpConstants.ResetStrings.Contains(query.Args.Last()))
        {
            binding.Reset();
            return true;
        }

        if (query.Args.Length < 1)
        {
            Imperium.IO.Send("This command requires 1 argument.");
            return false;
        }

        if (!float.TryParse(query.Args.Last(), out var value))
        {
            Imperium.IO.Send("The specified value is not a valid decimal.");
            return false;
        }

        if (minValue.HasValue) value = Mathf.Max(value, minValue.Value);
        if (maxValue.HasValue) value = Mathf.Min(value, maxValue.Value);
        binding.Set(value);

        Imperium.IO.Send($"{Name} has been set to {binding.Value:0.#}{valueUnit}.");
        return true;
    }

    internal override string GetDisplayName(ConsoleQuery query)
    {
        if (query.Keyword == QueryKeyword.Reset ||
            query.Args.Length > 0 && ImpConstants.ResetStrings.Contains(query.Args.Last()))
        {
            return $"Reset {Name} to {binding.DefaultValue:0.#}{valueUnit}";
        }

        if (query.Args.Length > 0)
        {
            if (float.TryParse(query.Args.Last(), out var value))
            {
                if (minValue.HasValue) value = Mathf.Max(value, minValue.Value);
                if (maxValue.HasValue) value = Mathf.Min(value, maxValue.Value);
                return $"Set {Name} to {value:0.#}{valueUnit}";
            }
        }

        return $"Set {Name}{GetGuidanceString()}";
    }

    internal override void OnIconClick()
    {
        if (interfacePath == null) return;

        Imperium.Interface.Get<ImperiumUI>().HighlightElement(interfacePath);
    }

    private string GetGuidanceString()
    {
        if (!minValue.HasValue && !maxValue.HasValue) return "";
        if (minValue.HasValue && !maxValue.HasValue) return $" <color={ImpConstants.GreyedOutColor}>(Min: {minValue:0.#})";
        if (maxValue.HasValue && !minValue.HasValue) return $" <color={ImpConstants.GreyedOutColor}>(Max: {maxValue:0.#})";

        return $" <color={ImpConstants.GreyedOutColor}>(Min: {minValue:0.#} Max: {maxValue:0.#})";
    }
}