using Imperium.Util;
using Librarium.Binding;
using UnityEngine;

namespace Imperium.Console.Commands;

internal abstract class ImpCommand(
    string name,
    Sprite customIcon = null,
    bool enabledBindingInverted = false,
    IBinding<bool> enabledBinding = null
)
{
    internal readonly string Name = name;

    internal virtual string DisplayType => "Command";
    internal virtual Sprite Icon => customIcon ?? ImpAssets.IconCommandSetting;

    protected readonly Sprite customIcon = customIcon;

    internal virtual string GetDisplayName(ConsoleQuery query) => Name;

    internal abstract bool Execute(ConsoleQuery query);

    internal virtual bool IsEnabled()
    {
        if (enabledBinding == null) return true;

        switch (enabledBinding.Value)
        {
            case false when enabledBindingInverted:
            case true when !enabledBindingInverted:
                return false;
            default:
                return true;
        }
    }

    internal virtual void OnIconClick()
    {
    }
}