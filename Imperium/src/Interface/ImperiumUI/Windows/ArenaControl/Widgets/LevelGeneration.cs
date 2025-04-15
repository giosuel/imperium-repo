#region

using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Librarium.Binding;
using Photon.Pun;
using TMPro;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ArenaControl.Widgets;

public class LevelGeneration : ImpWidget
{
    private TMP_InputField levelSeedInput;
    private TMP_InputField levelMapSizeInput;
    private TMP_InputField levelModuleAmountInput;

    protected override void InitWidget()
    {
        var disabledBinding = ImpBinaryBinding.CreateOr([
            (Imperium.IsArenaLoaded, false),
            (new ImpBinding<bool>(!PhotonNetwork.IsMasterClient), false)
        ]);

        levelSeedInput = ImpInput.Bind(
            "Seed/Input",
            transform,
            Imperium.GameManager.CustomSeed,
            theme: theme,
            interactableInvert: true,
            negativeIsEmpty: true,
            interactableBindings: disabledBinding
        );
        ImpUtils.Interface.BindInputInteractable(disabledBinding, transform.Find("Seed"), true);

        ImpButton.Bind(
            "Seed/Reset",
            transform,
            () => Imperium.GameManager.CustomSeed.Reset(),
            theme: theme,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        levelMapSizeInput = ImpInput.Bind(
            "MapSize/Input",
            transform,
            Imperium.GameManager.CustomMapSize,
            theme: theme,
            interactableInvert: true,
            negativeIsEmpty: true,
            interactableBindings: disabledBinding
        );
        ImpUtils.Interface.BindInputInteractable(disabledBinding, transform.Find("MapSize"), true);

        ImpButton.Bind(
            "MapSize/Reset",
            transform,
            () => Imperium.GameManager.CustomMapSize.Reset(),
            theme: theme,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        levelModuleAmountInput = ImpInput.Bind(
            "ModuleAmount/Input",
            transform,
            Imperium.GameManager.CustomModuleAmount,
            theme: theme,
            interactableInvert: true,
            negativeIsEmpty: true,
            interactableBindings: disabledBinding
        );
        ImpUtils.Interface.BindInputInteractable(disabledBinding, transform.Find("ModuleAmount"), true);

        ImpButton.Bind(
            "ModuleAmount/Reset",
            transform,
            () => Imperium.GameManager.CustomModuleAmount.Reset(),
            theme: theme,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        // var options = Imperium.RoundManager.dungeonFlowTypes
        //     .Select(flow => new TMP_Dropdown.OptionData(GetDungeonFlowDisplayName(flow.dungeonFlow.name)))
        //     .ToList();

        var dungeonFlowDropdown = transform.Find("Map/Dropdown").GetComponent<TMP_Dropdown>();
        // dungeonFlowDropdown.options = options;
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