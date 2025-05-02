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
        InitQuotaAndCredits();

        RegisterWidget<ArenaSettings>(transform, "Left");
    }

    protected override void OnThemePrimaryUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("Left", Variant.DARKER)
        );
    }

    private void InitQuotaAndCredits()
    {
        ImpInput.Bind(
            "Right/GroupCurrency/Input",
            transform,
            Imperium.GameManager.GroupCurrency,
            min: 0,
            theme: theme
        );

        ImpInput.Bind(
            "Right/TotalHaul/Input",
            transform,
            Imperium.GameManager.TotalHaul,
            min: 0,
            theme: theme
        );

        ImpToggle.Bind(
            "Right/LowHaul",
            transform,
            Imperium.GameManager.LowHaul,
            theme
        );

        ImpButton.Bind(
            "Right/ShopButtons/BuyItems",
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