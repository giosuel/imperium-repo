using System;
using Imperium.Interface.ImperiumUI;
using Imperium.Util;
using Librarium.Binding;
using UnityEngine;

namespace Imperium.Console.Commands;

/// <summary>
/// Console command to execute an arbitrary action.
/// </summary>
/// <param name="name">The display name of the action</param>
/// <param name="onExec">A function that is called when the command is executed</param>
/// <param name="customIcon">A custom icon that is displayed next to the command</param>
internal class ImpCommandAction(
    string name,
    Action<ConsoleQuery> onExec,
    Func<ConsoleQuery, string> displayNameOverride,
    string interfacePath = null,
    Sprite customIcon = null,
    bool enabledBindingInverted = false,
    IBinding<bool> enabledBinding = null
) : ImpCommand(name, customIcon, enabledBindingInverted, enabledBinding)
{
    internal override Sprite Icon => customIcon ?? ImpAssets.IconCommandAction;

    internal override bool Execute(ConsoleQuery query)
    {
        onExec?.Invoke(query);

        return true;
    }

    internal override void OnIconClick()
    {
        if (interfacePath == null) return;

        Imperium.Interface.Get<ImperiumUI>().HighlightElement(interfacePath);
    }

    internal override string GetDisplayName(ConsoleQuery query)
    {
        return displayNameOverride?.Invoke(query) ?? base.GetDisplayName(query);
    }
}