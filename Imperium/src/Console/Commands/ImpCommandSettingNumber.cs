#region

using System;
using System.Linq;
using Imperium.Core;
using Imperium.Interface.ImperiumUI;
using Imperium.Util;
using Librarium.Binding;
using UnityEngine;

#endregion

namespace Imperium.Console.Commands;

/// <summary>
///     Console command to change the value of an int binding.
/// </summary>
/// <param name="name">The display name of the command</param>
/// <param name="binding">The binding that will be changed</param>
/// <param name="minValue">The min value that is allowed in the binding</param>
/// <param name="maxValue">The max value that is allowed in the binding</param>
/// <param name="valueUnit">The displayed unit of the value (e.g. % or units)</param>
/// <param name="customIcon">A custom icon that is displayed next to the command</param>
internal class ImpCommandSettingNumber(
    string name,
    IBinding<int> binding,
    int? minValue,
    int? maxValue,
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

        if (!int.TryParse(query.Args.Last(), out var value))
        {
            Imperium.IO.Send("The specified value is not a valid integer.");
            return false;
        }

        if (minValue.HasValue) value = Math.Max(value, minValue.Value);
        if (maxValue.HasValue) value = Math.Min(value, maxValue.Value);
        binding.Set(value);

        Imperium.IO.Send($"{Name} has been set to {binding.Value}{valueUnit}.");
        return true;
    }

    internal override string GetDisplayName(ConsoleQuery query)
    {
        if (query.Keyword == QueryKeyword.Reset ||
            query.Args.Length > 0 && ImpConstants.ResetStrings.Contains(query.Args.Last()))
        {
            return $"Reset {Name} to {binding.DefaultValue}{valueUnit}";
        }

        if (query.Args.Length > 0)
        {
            if (int.TryParse(query.Args.Last(), out var value))
            {
                if (minValue.HasValue) value = Math.Max(value, minValue.Value);
                if (maxValue.HasValue) value = Math.Min(value, maxValue.Value);
                return $"Set {Name} to {value}{valueUnit}";
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
        if (minValue.HasValue && !maxValue.HasValue) return $" <color={ImpConstants.GreyedOutColor}>(Min: {minValue})";
        if (maxValue.HasValue && !minValue.HasValue) return $" <color={ImpConstants.GreyedOutColor}>(Max: {maxValue})";

        return $" <color={ImpConstants.GreyedOutColor}>(Min: {minValue} Max: {maxValue})";
    }
}