#region

using Imperium.Interface.Common;
using Imperium.Types;

#endregion

namespace Imperium.Interface.MapUI;

internal class MinimapSettings : BaseUI
{
    protected override void InitUI()
    {
        ImpToggle.Bind(
            "Content/Gizmos/ShowInfoPanel",
            transform,
            Imperium.Settings.Map.MinimapInfoPanel,
            theme: theme
        );

        ImpToggle.Bind(
            "Content/Gizmos/ShowLocationPanel",
            transform,
            Imperium.Settings.Map.MinimapLocationPanel,
            theme: theme
        );

        // ImpSlider.Bind(
        //     path: "Content/Scale",
        //     container: transform,
        //     valueBinding: Imperium.Settings.Map.MinimapScale,
        //     handleFormatter: value => $"{value:0.0}",
        //     theme: theme
        // );
    }

    protected override void OnThemePrimaryUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("Content/Width/Overlay", Variant.LIGHTER),
            new StyleOverride("Content/Height/Overlay", Variant.LIGHTER)
        );
    }
}