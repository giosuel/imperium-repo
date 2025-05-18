using System;
using System.Linq;
using Imperium.Core;
using Imperium.Interface.ImperiumUI;
using Imperium.Util;
using Librarium.Binding;
using UnityEngine;

namespace Imperium.Console.Commands;

/// <summary>
/// Console command to change the value of a string binding.
/// </summary>
/// <param name="name"></param>
/// <param name="binding"></param>
/// <param name="customIcon"></param>
internal class ImpCommandSettingString(
    string name,
    IBinding<string> binding,
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

        binding.Set(query.Args.Last());

        Imperium.IO.Send($"{Name} has been set to {binding.Value}.");
        return true;
    }

    internal override string GetDisplayName(ConsoleQuery query)
    {
        if (query.Keyword == QueryKeyword.Reset ||
            query.Args.Length > 0 && ImpConstants.ResetStrings.Contains(query.Args.Last()))
        {
            return $"Reset {Name} to {binding.DefaultValue}";
        }

        return query.Args.Length > 0 ? $"Set {Name} to {query.Args.Last()}" : $"Set {Name}";
    }

    internal override void OnIconClick()
    {
        if (interfacePath == null) return;

        Imperium.Interface.Get<ImperiumUI>().HighlightElement(interfacePath);
    }
}