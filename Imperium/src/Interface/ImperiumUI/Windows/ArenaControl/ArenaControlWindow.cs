#region

using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.ArenaControl.Widgets;
using Imperium.Interface.ImperiumUI.Windows.ControlCenter.Widgets;
using Imperium.Types;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ArenaControl;

internal class ArenaControlWindow : ImperiumWindow
{
    private Transform content;

    protected override void InitWindow()
    {
        InitQuotaAndCredits();

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

    private void InitQuotaAndCredits()
    {
        ImpInput.Bind(
            "Left/GroupCurrency/Input",
            transform,
            Imperium.GameManager.GroupCurrency,
            min: 0,
            theme: theme
        );

        ImpInput.Bind(
            "Left/TotalHaul/Input",
            transform,
            Imperium.GameManager.TotalHaul,
            min: 0,
            theme: theme
        );

        ImpToggle.Bind(
            "Left/LowHaul",
            transform,
            Imperium.GameManager.LowHaul,
            theme
        );

        ImpButton.Bind(
            "Left/ShopButtons/BuyItems",
            transform,
            () =>
            {
                StatsManager.instance.BuyAllItems();
                SemiFunc.StatSyncAll();
            },
            theme
        );
    }
}