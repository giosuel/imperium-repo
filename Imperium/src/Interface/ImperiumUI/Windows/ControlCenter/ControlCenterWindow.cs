#region

using Imperium.Interface.ImperiumUI.Windows.ControlCenter.Widgets;
using Imperium.Types;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ControlCenter;

internal class ControlCenterWindow : ImperiumWindow
{
    private Transform content;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        RegisterWidget<GrabberControl>(content, "Left");
        RegisterWidget<PlayerSettings>(content, "Right");

        if (Random.Range(0, 100) >= 99) titleBox.Find("Title").GetComponent<TMP_Text>().text = "Emporium Control Panel";
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("Separator", Variant.DARKER)
        );
    }
}