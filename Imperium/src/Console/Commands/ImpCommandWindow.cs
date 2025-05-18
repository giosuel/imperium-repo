#region

using Imperium.Core;
using Imperium.Interface.ImperiumUI;
using UnityEngine;

#endregion

namespace Imperium.Console.Commands;

/// <summary>
///     Console command to open a window within the Imperium UI.
/// </summary>
/// <param name="name">The name of the window</param>
/// <typeparam name="T">The type of the window</typeparam>
internal sealed class ImpCommandWindow<T>(string name) : ImpCommand(name) where T : ImperiumWindow
{
    internal override string DisplayType => "Open Window";
    internal override Sprite Icon => ImpConstants.WindowIconMap[typeof(T)];

    private readonly string windowName = name;

    internal override bool Execute(ConsoleQuery query)
    {
        Imperium.Interface.Get<ImperiumUI>().Get<T>().OpenAndFocus();

        return false;
    }

    internal override string GetDisplayName(ConsoleQuery query) => $"Open {windowName}";

    internal override bool IsEnabled() => Imperium.Interface.Get<ImperiumUI>().CanOpen<T>();
}