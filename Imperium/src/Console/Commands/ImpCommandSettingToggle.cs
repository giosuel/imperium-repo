using Imperium.Core;
using Imperium.Interface.ImperiumUI;
using Imperium.Util;
using Librarium.Binding;
using UnityEngine;

namespace Imperium.Console.Commands;

internal class ImpCommandSettingToggle(
    string name,
    IBinding<bool> binding,
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
            query.Args.Length > 0 && ImpConstants.ResetStrings.Contains(query.Args[0]))
        {
            binding.Reset();
            Imperium.IO.Send($"{Name} has been reset.");
            return true;
        }

        if (
            query.Keyword == QueryKeyword.Enable ||
            query.Args.Length > 0 && ImpConstants.TrueStrings.Contains(query.Args[0])
        )
        {
            // Explicitly set binding to true
            binding.Set(true);
        }
        else if (
            query.Keyword == QueryKeyword.Disable ||
            query.Args.Length > 0 && ImpConstants.FalseStrings.Contains(query.Args[0])
        )
        {
            // Explicitly set binding to false
            binding.Set(false);
        }
        else
        {
            // Explicitly or implicitly toggle binding
            binding.Set(!binding.Value);
        }

        Imperium.IO.Send(binding.Value ? $"{Name} has been enabled." : $"{Name} has been disabled.");
        return true;
    }

    internal override string GetDisplayName(ConsoleQuery query)
    {
        if (query.Keyword == QueryKeyword.Reset ||
            query.Args.Length > 0 && ImpConstants.ResetStrings.Contains(query.Args[0]))
        {
            return $"Reset {Name} to {binding.DefaultValue}";
        }

        if (
            query.Keyword == QueryKeyword.Enable ||
            query.Args.Length > 0 && ImpConstants.TrueStrings.Contains(query.Args[0])
        )
        {
            return $"Enable {Name}";
        }

        if (
            query.Keyword == QueryKeyword.Disable ||
            query.Args.Length > 0 && ImpConstants.FalseStrings.Contains(query.Args[0])
        )
        {
            return $"Disable {Name}";
        }

        return binding.Value ? $"Disable {Name}" : $"Enable {Name}";
    }

    internal override void OnIconClick()
    {
        if (interfacePath == null) return;

        Imperium.Interface.Get<ImperiumUI>().HighlightElement(interfacePath);
    }
}