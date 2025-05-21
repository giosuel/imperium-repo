#region

using Imperium.Interface.Common;
using Imperium.Interface.ImperiumUI.Windows.ArenaControl.Widgets;
using Imperium.Types;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ArenaControl;

internal class GameControlWindow : ImperiumWindow
{
    protected override void InitWindow()
    {
        InitQuotaAndCredits();

        RegisterWidget<GameSettings>(transform, "Left");
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
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
            "Right/GroupCurrency",
            transform,
            Imperium.GameManager.GroupCurrency,
            min: 0,
            theme: theme
        );

        ImpInput.Bind(
            "Right/TotalHaul",
            transform,
            Imperium.GameManager.TotalHaul,
            min: 0,
            theme: theme
        );

        ImpToggle.Bind(
            "Right/LowHaul",
            transform,
            Imperium.GameManager.LowHaul,
            theme: theme
        );

        ImpButton.Bind(
            "Right/ShopButtons/BuyItems",
            transform,
            () =>
            {
                StatsManager.instance.BuyAllItems();
                SemiFunc.StatSyncAll();
            },
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Buy All Items",
                Description = "Caution! Requires a level reload.",
                Tooltip = tooltip
            }
        );
    }
}