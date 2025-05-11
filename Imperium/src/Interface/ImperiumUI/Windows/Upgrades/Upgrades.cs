#region

using System.Collections.Generic;
using HarmonyLib;
using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Types;
using Librarium.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Upgrades;

internal class Upgrades : ImperiumWindow
{
    private Transform content;

    private GameObject template;

    protected override void InitWindow()
    {
        content = transform.Find("Content/Viewport/Content");

        template = content.Find("Template").gameObject;
        template.SetActive(false);

        BuildInterface();
    }

    private void BuildInterface()
    {
    }

    private void BuildElement(string dictionaryName)
    {
        var element = Instantiate(template, content);
        element.SetActive(true);

        var upgradeBinding = new ImpExternalBinding<int, int>(
            () => StatsManager.instance.dictionaryOfDictionaries[dictionaryName][Imperium.Player.steamID]
        );

        var upgradeDisplayName = ImpConstants.PlayerUpgradeNameMap.GetValueOrDefault(dictionaryName) ?? dictionaryName;
        element.transform.Find("Text").GetComponent<TMP_Text>().text = upgradeDisplayName;

        ImpInput.Bind(
            "Input",
            element.transform,
            upgradeBinding,
            theme: theme
        );

        ImpButton.Bind(
            "Minus",
            element.transform,
            onClick: () => upgradeBinding.Set(upgradeBinding.Value - 1),
            theme: theme
        );

        ImpButton.Bind(
            "Plus",
            element.transform,
            onClick: () => upgradeBinding.Set(upgradeBinding.Value + 1),
            theme: theme
        );

        ImpButton.Bind(
            "Reset",
            element.transform,
            onClick: () => upgradeBinding.Refresh(),
            theme: theme
        );
    }

    protected override void OnOpen()
    {
        Imperium.PlayerManager.PlayerUpgradeBinding.Values.Do(binding => binding.Refresh());
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            content,
            new StyleOverride("LevelOverride/Dropdown", Variant.FOREGROUND),
            new StyleOverride("LevelOverride/Dropdown/Arrow", Variant.FOREGROUND),
            new StyleOverride("LevelOverride/Dropdown/Template", Variant.DARKER),
            new StyleOverride("LevelOverride/Dropdown/Template/Viewport/Content/Item/Background", Variant.DARKER),
            new StyleOverride("LevelOverride/Dropdown/Template/Scrollbar", Variant.DARKEST),
            new StyleOverride("LevelOverride/Dropdown/Template/Scrollbar/SlidingArea/Handle", Variant.LIGHTER),
            new StyleOverride("ModuleOverride/Dropdown", Variant.FOREGROUND),
            new StyleOverride("ModuleOverride/Dropdown/Arrow", Variant.FOREGROUND),
            new StyleOverride("ModuleOverride/Dropdown/Template", Variant.DARKER),
            new StyleOverride("ModuleOverride/Dropdown/Template/Viewport/Content/Item/Background", Variant.DARKER),
            new StyleOverride("ModuleOverride/Dropdown/Template/Scrollbar", Variant.DARKEST),
            new StyleOverride("ModuleOverride/Dropdown/Template/Scrollbar/SlidingArea/Handle", Variant.LIGHTER),
            new StyleOverride("ModuleType/Dropdown", Variant.FOREGROUND),
            new StyleOverride("ModuleType/Dropdown/Arrow", Variant.FOREGROUND),
            new StyleOverride("ModuleType/Dropdown/Template", Variant.DARKER),
            new StyleOverride("ModuleType/Dropdown/Template/Viewport/Content/Item/Background", Variant.DARKER),
            new StyleOverride("ModuleType/Dropdown/Template/Scrollbar", Variant.DARKEST),
            new StyleOverride("ModuleType/Dropdown/Template/Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );
    }
}