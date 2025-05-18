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
            case true when !enabledBindingInverted:
            case false when enabledBindingInverted:
                return true;
            default:
                return false;
        }
    }

    internal virtual void OnIconClick()
    {
    }
}