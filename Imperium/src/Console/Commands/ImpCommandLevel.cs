#region

using System.Linq;
using Imperium.Util;
using UnityEngine;

#endregion

namespace Imperium.Console.Commands;

/// <summary>
///     Console command to execute an arbitrary action.
/// </summary>
/// <param name="level">The level to load when the command is executed</param>
internal class ImpCommandLevel(Level level) : ImpCommand(level.NarrativeName)
{
    internal override string DisplayType => "Load Level";
    internal override Sprite Icon => ImpAssets.IconCommandReload;

    internal override bool Execute(ConsoleQuery query)
    {
        if (query.Args.Length > 0 && int.TryParse(query.Args.Last(), out var levelNumber))
        {
            Core.Lifecycle.GameManager.LoadLevel(level, levelNumber);
            return true;
        }

        Core.Lifecycle.GameManager.LoadLevel(level, null);
        return true;
    }

    internal override string GetDisplayName(ConsoleQuery query)
    {
        if (query.Args.Length > 0 && int.TryParse(query.Args.Last(), out var levelNumber))
        {
            return $"Load {Name} as Level #{levelNumber}";
        }

        return $"Load {Name}";
    }

    internal override bool IsEnabled() => Imperium.IsGameLevel.Value;
}