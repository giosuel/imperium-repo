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

internal class UpgradesWindow : ImperiumWindow
{
    private Transform content;

    private GameObject template;

    protected override void InitWindow()
    {
        content = transform.Find("Content/Viewport/Content");

        template = content.Find("Template").gameObject;
        template.SetActive(false);

        BuildElement("playerUpgradeHealth");
        BuildElement("playerUpgradeStrength");
        BuildElement("playerUpgradeStamina");
        BuildElement("playerUpgradeSpeed");
        BuildElement("playerUpgradeExtraJump");
        BuildElement("playerUpgradeRange");
        BuildElement("playerUpgradeThrow");
        BuildElement("playerUpgradeLaunch");
        BuildElement("playerUpgradeMapPlayerCount");
    }

    private void BuildElement(string upgradeName)
    {
        var element = Instantiate(template, content);
        element.SetActive(true);

        var upgradeBinding = new ImpExternalBinding<int, int>(
            () => StatsManager.instance.dictionaryOfDictionaries[upgradeName][Imperium.Player.steamID]
        );

        upgradeBinding.OnUpdateSecondary += value =>
        {
            StatsManager.instance.dictionaryOfDictionaries[upgradeName][Imperium.Player.steamID] = value;
        };

        var upgradeDisplayName = ImpConstants.PlayerUpgradeNameMap.GetValueOrDefault(upgradeName) ?? upgradeName;
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
            onClick: () => upgradeBinding.Reset(),
            theme: theme
        );

        Imperium.PlayerManager.PlayerUpgradeBinding.Add(upgradeName, upgradeBinding);
    }

    protected override void OnOpen()
    {
        // We use secondary here to skip setting the value again (see above)
        Imperium.PlayerManager.PlayerUpgradeBinding.Do(entry => entry.Value.Set(
            StatsManager.instance.dictionaryOfDictionaries[entry.Key][Imperium.Player.steamID],
            invokeSecondary: false
        ));
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            content,
            new StyleOverride("Scrollbar", Variant.DARKEST),
            new StyleOverride("Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );
    }
}