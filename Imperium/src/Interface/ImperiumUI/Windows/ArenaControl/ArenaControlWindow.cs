#region

using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.ArenaControl.Widgets;
using Imperium.Types;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ArenaControl;

internal class ArenaControlWindow : ImperiumWindow
{
    private Transform content;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        RegisterWidget<LevelGeneration>(transform, "Left/Generation");
        RegisterWidget<ArenaSettings>(transform, "Right");
    }

    protected override void OnThemePrimaryUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("Right", Variant.DARKER)
        );
    }
}