#region

using System;
using System.Linq;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Librarium.Binding;
using TMPro;
using Unity.VisualScripting;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.ControlCenter.Widgets;

public class LevelGeneration : ImpWidget
{
    protected override void InitWidget()
    {
        var disabledBinding = new ImpBinaryBinding(!SemiFunc.IsMasterClientOrSingleplayer());

        ImpInput.Bind(
            "Numbers/LevelSize/Input",
            transform,
            Imperium.GameManager.CustomLevelSize,
            theme: theme,
            interactableInvert: true,
            negativeIsEmpty: true,
            interactableBindings: disabledBinding
        );
        ImpUtils.Interface.BindInputInteractable(disabledBinding, transform.Find("Numbers/LevelSize"), true);

        ImpButton.Bind(
            "Numbers/LevelSize/Reset",
            transform,
            () => Imperium.GameManager.CustomLevelSize.Reset(),
            theme: theme,
            isIconButton: true,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        ImpInput.Bind(
            "Numbers/ModuleAmount/Input",
            transform,
            Imperium.GameManager.CustomModuleAmount,
            theme: theme,
            interactableInvert: true,
            negativeIsEmpty: true,
            interactableBindings: disabledBinding
        );
        ImpUtils.Interface.BindInputInteractable(disabledBinding, transform.Find("Numbers/ModuleAmount"), true);

        ImpButton.Bind(
            "Numbers/ModuleAmount/Reset",
            transform,
            () => Imperium.GameManager.CustomModuleAmount.Reset(),
            theme: theme,
            isIconButton: true,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        ImpInput.Bind(
            "LevelNumber/Input",
            transform,
            Imperium.GameManager.CustomLevelNumber,
            theme: theme,
            interactableInvert: true,
            negativeIsEmpty: true,
            interactableBindings: disabledBinding
        );
        ImpUtils.Interface.BindInputInteractable(disabledBinding, transform.Find("LevelNumber"), true);

        ImpButton.Bind(
            "LevelNumber/Reset",
            transform,
            () => Imperium.GameManager.CustomLevelNumber.Reset(),
            theme: theme,
            isIconButton: true,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        ImpButton.Bind(
            "ReloadLevel",
            transform,
            () => RunManager.instance.ChangeLevel(_completedLevel: false, _levelFailed: false),
            theme: theme,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        ImpButton.Bind(
            "AdvanceLevel",
            transform,
            () => RunManager.instance.ChangeLevel(_completedLevel: true, _levelFailed: false),
            theme: theme,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        InitLevelOverride(disabledBinding);
        InitModuleOverride(disabledBinding);

        // Module type should also be disabled if no override module is selected
        var noModuleOverride = new ImpBinaryBinding(true);
        Imperium.GameManager.ModuleOverride.onUpdate += module => noModuleOverride.Set(module == "");

        InitModuleType(ImpBinaryBinding.CreateOr([
            (disabledBinding, false),
            (noModuleOverride, false)
        ]));
    }

    private void InitLevelOverride(ImpBinaryBinding disabledBinding)
    {
        var options = Imperium.ObjectManager.LoadedLevels.Value
            .Select(level => new TMP_Dropdown.OptionData(level.NarrativeName))
            .ToList();

        var dropdown = transform.Find("LevelOverride/Dropdown").GetComponent<TMP_Dropdown>();
        dropdown.options = options;
        dropdown.value = -1;

        dropdown.onValueChanged.AddListener(value =>
        {
            if (value < 0) return;

            var customLevel = Imperium.ObjectManager.LoadedLevels.Value[value];
            Imperium.GameManager.LevelOverride.Set(customLevel.NarrativeName, invokeSecondary: false);
        });

        Imperium.GameManager.LevelOverride.onUpdateSecondary += value =>
        {
            if (value == "")
            {
                dropdown.value = -1;
                return;
            }

            for (var i = 0; i < Imperium.ObjectManager.LoadedLevels.Value.Count; i++)
            {
                var level = Imperium.ObjectManager.LoadedLevels.Value[i];
                if (level.NarrativeName == value)
                {
                    dropdown.value = i;
                    break;
                }
            }
        };
        ImpUtils.Interface.BindDropdownInteractable(disabledBinding, transform.Find("LevelOverride"), true);

        ImpButton.Bind(
            "LevelOverride/Reset",
            transform,
            () => Imperium.GameManager.LevelOverride.Reset(),
            theme: theme,
            isIconButton: true,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        transform.Find("LevelOverrideTitle").AddComponent<ImpTooltipTrigger>().Init(new TooltipDefinition
        {
            Title = "Level Override",
            Description = "Forces the game to always load\nthe selected level.",
            Tooltip = tooltip
        });
    }

    private void InitModuleOverride(ImpBinaryBinding disabledBinding)
    {
        var options = Imperium.ObjectManager.LoadedModules.Value
            .Select(module => new TMP_Dropdown.OptionData(NormalizeModuleName(module.name)))
            .ToList();

        var dropdown = transform.Find("ModuleOverride/Dropdown").GetComponent<TMP_Dropdown>();
        dropdown.options = options;
        dropdown.value = -1;

        dropdown.onValueChanged.AddListener(value =>
        {
            if (value < 0) return;

            var customModule = Imperium.ObjectManager.LoadedModules.Value[value];
            Imperium.GameManager.ModuleOverride.Set(customModule.name, invokeSecondary: false);
        });

        Imperium.GameManager.ModuleOverride.onUpdateSecondary += value =>
        {
            if (value == "")
            {
                dropdown.value = -1;
                return;
            }

            for (var i = 0; i < Imperium.ObjectManager.LoadedModules.Value.Count; i++)
            {
                var module = Imperium.ObjectManager.LoadedModules.Value[i];
                if (module.name == value)
                {
                    dropdown.value = i;
                    break;
                }
            }
        };
        ImpUtils.Interface.BindDropdownInteractable(disabledBinding, transform.Find("ModuleOverride"), true);

        ImpButton.Bind(
            "ModuleOverride/Reset",
            transform,
            () => Imperium.GameManager.ModuleOverride.Reset(),
            theme: theme,
            isIconButton: true,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        transform.Find("ModuleOverrideTitle").AddComponent<ImpTooltipTrigger>().Init(new TooltipDefinition
        {
            Title = "Module Override",
            Description = "Makes the level to be generated from a single module.\nCaution! Can cause faulty levels.",
            Tooltip = tooltip
        });
    }

    private void InitModuleType(ImpBinaryBinding disabledBinding)
    {
        var options = Enum.GetValues(typeof(Module.Type))
            .Cast<Module.Type>()
            .OrderBy(value => value)
            .Select(type => new TMP_Dropdown.OptionData(type.ToString()))
            .ToList();

        var dropdown = transform.Find("ModuleType/Dropdown").GetComponent<TMP_Dropdown>();
        dropdown.options = options;
        dropdown.value = 0;

        dropdown.onValueChanged.AddListener(value =>
        {
            Imperium.GameManager.OverrideModuleType.Set(value, invokeSecondary: false);
        });

        Imperium.GameManager.OverrideModuleType.onUpdateSecondary += value => dropdown.value = value;
        ImpUtils.Interface.BindDropdownInteractable(disabledBinding, transform.Find("ModuleType"), true);

        ImpButton.Bind(
            "ModuleType/Reset",
            transform,
            () => Imperium.GameManager.OverrideModuleType.Reset(),
            theme: theme,
            isIconButton: true,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        transform.Find("ModuleTypeTitle").AddComponent<ImpTooltipTrigger>().Init(new TooltipDefinition
        {
            Title = "Module Type",
            Description = "The type of module to generate with the override module.",
            Tooltip = tooltip
        });
    }

    private static string NormalizeModuleName(string moduleName) => moduleName.Replace("Module - ", "").Replace(" - ", " ");

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
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