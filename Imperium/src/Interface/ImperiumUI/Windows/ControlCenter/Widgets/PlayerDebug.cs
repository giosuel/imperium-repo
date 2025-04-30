#region

using System.Linq;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Librarium.Binding;
using Photon.Pun;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ControlCenter.Widgets;

public class PlayerDebug : ImpWidget
{
    protected override void InitWidget()
    {
        var disabledBinding = ImpBinaryBinding.CreateOr([
            (Imperium.IsArenaLoaded, false),
            (new ImpBinding<bool>(!PhotonNetwork.IsMasterClient), false)
        ]);

        ImpInput.Bind(
            "DebugPlayer/Input",
            transform,
            Imperium.GameManager.CustomMapSize,
            theme: theme,
            interactableInvert: true,
            negativeIsEmpty: true,
            interactableBindings: disabledBinding
        );
        ImpUtils.Interface.BindInputInteractable(disabledBinding, transform.Find("MapSize"), true);

        ImpButton.Bind(
            "DebugPlayer/Reset",
            transform,
            () => Imperium.GameManager.CustomMapSize.Reset(),
            theme: theme,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        var options = Imperium.PlayerManager.DebugComputers
            .Select(computer => new TMP_Dropdown.OptionData(computer.name))
            .ToList();

        var dungeonFlowDropdown = transform.Find("DebugPlayer/Dropdown").GetComponent<TMP_Dropdown>();
        dungeonFlowDropdown.options = options;
        dungeonFlowDropdown.value = -1;

        dungeonFlowDropdown.onValueChanged.AddListener(value =>
        {
            Imperium.GameManager.CustomDungeonFlow.Set(value);
        });

        ImpUtils.Interface.BindDropdownInteractable(disabledBinding, transform.Find("Map"), true);

        ImpButton.Bind(
            "DungeonFlow/Reset",
            transform,
            () => Imperium.GameManager.CustomDungeonFlow.Reset(),
            theme: theme,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("Map/Dropdown", Variant.FOREGROUND),
            new StyleOverride("Map/Dropdown/Arrow", Variant.FOREGROUND),
            new StyleOverride("Map/Dropdown/Template", Variant.DARKER),
            new StyleOverride("Map/Dropdown/Template/Viewport/Content/Item/Background", Variant.DARKER),
            new StyleOverride("Map/Dropdown/Template/Scrollbar", Variant.DARKEST),
            new StyleOverride("Map/Dropdown/Template/Scrollbar/SlidingArea/Handle", Variant.LIGHTER)
        );
    }

    protected override void OnOpen()
    {
        // levelSeedInput.text = Imperium.IsSceneLoaded.Value
        //     ? Imperium.StartOfRound.randomMapSeed.ToString()
        //     : Imperium.GameManager.CustomSeed.Value != -1
        //         ? Imperium.GameManager.CustomSeed.Value.ToString()
        //         : "";
        //
        // levelMapSizeInput.text = Imperium.GameManager.CustomMapSize.Value > -1
        //     ? Imperium.GameManager.CustomMapSize.Value.ToString(CultureInfo.InvariantCulture)
        //     : Imperium.IsSceneLoaded.Value
        //         ? Imperium.RoundManager.currentLevel.factorySizeMultiplier.ToString(CultureInfo.InvariantCulture)
        //         : "";
    }
}