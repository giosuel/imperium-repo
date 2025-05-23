#region

using System;
using System.Linq;
using Imperium.Interface.Common;
using Imperium.Types;
using Librarium.Binding;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.LevelGeneration;

internal class LevelGeneration : ImperiumWindow
{
    private Transform content;

    protected override void InitWindow()
    {
        content = transform.Find("Content");

        var disabledBinding = new ImpBinaryBinding(!SemiFunc.IsMasterClientOrSingleplayer());

        ImpInput.Bind(
            "Numbers/LevelSize",
            content,
            Imperium.GameManager.CustomLevelSize,
            negativeIsEmpty: true,
            theme: theme,
            parentWindow: this,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        ImpInput.Bind(
            "Numbers/ModuleAmount",
            content,
            Imperium.GameManager.CustomModuleAmount,
            negativeIsEmpty: true,
            theme: theme,
            parentWindow: this,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        ImpInput.Bind(
            "LevelNumber",
            content,
            Imperium.GameManager.CustomLevelNumber,
            negativeIsEmpty: true,
            theme: theme,
            parentWindow: this,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        ImpButton.Bind(
            "ReloadLevel",
            content,
            Core.Lifecycle.GameManager.ReloadLevel,
            theme: theme,
            parentWindow: this,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        ImpButton.Bind(
            "AdvanceLevel",
            content,
            Core.Lifecycle.GameManager.AdvanceLevel,
            theme: theme,
            parentWindow: this,
            interactableInvert: true,
            interactableBindings: disabledBinding
        );

        InitLevelOverride(disabledBinding);
        InitValuableSpawns(disabledBinding);
        InitModuleOverride(disabledBinding);

        // Module type should also be disabled if no override module is selected
        var noModuleOverride = new ImpBinaryBinding(true);
        Imperium.GameManager.ModuleOverride.OnUpdate += module => noModuleOverride.Set(module == "");

        InitModuleType(ImpBinaryBinding.CreateOr([
            (disabledBinding, false),
            (noModuleOverride, false)
        ]));
    }

    private void InitLevelOverride(ImpBinaryBinding disabledBinding)
    {
        // Create an intermediate binding to map the int value to a level name
        var dropdownBinding = new ImpBinding<int>(-1);
        dropdownBinding.OnUpdate += value =>
        {
            var newLevel = value >= 0 ? Imperium.ObjectManager.LoadedLevels.Value[value].NarrativeName : "";
            if (Imperium.GameManager.LevelOverride.Value != newLevel)
            {
                Imperium.GameManager.LevelOverride.Set(newLevel);
            }
        };

        Imperium.GameManager.LevelOverride.OnUpdate += value =>
        {
            if (value == "")
            {
                dropdownBinding.Set(-1);
                return;
            }

            for (var i = 0; i < Imperium.ObjectManager.LoadedLevels.Value.Count; i++)
            {
                if (Imperium.ObjectManager.LoadedLevels.Value[i].NarrativeName == value)
                {
                    dropdownBinding.Set(i);
                    break;
                }
            }
        };

        var options = Imperium.ObjectManager.LoadedLevels.Value.Select(level => level.NarrativeName);

        ImpDropdown.Bind(
            "LevelOverride",
            content,
            dropdownBinding,
            options,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Level Override",
                Description = "Forces the game to always load\nthe selected level.",
                Tooltip = tooltip
            },
            interactableInvert: true,
            interactableBindings: disabledBinding
        );
    }

    private void InitModuleOverride(ImpBinaryBinding disabledBinding)
    {
        // Create an intermediate binding to map the int value to a module name
        var dropdownBinding = new ImpBinding<int>(-1);
        dropdownBinding.OnUpdate += value =>
        {
            var newModule = value >= 0 ? Imperium.ObjectManager.LoadedModules.Value[value].name : "";
            if (Imperium.GameManager.ModuleOverride.Value != newModule)
            {
                Imperium.GameManager.ModuleOverride.Set(newModule);
            }
        };

        Imperium.GameManager.ModuleOverride.OnUpdate += value =>
        {
            if (value == "")
            {
                dropdownBinding.Set(-1);
                return;
            }

            for (var i = 0; i < Imperium.ObjectManager.LoadedModules.Value.Count; i++)
            {
                if (Imperium.ObjectManager.LoadedModules.Value[i].name == value)
                {
                    dropdownBinding.Set(i);
                    break;
                }
            }
        };

        var options = Imperium.ObjectManager.LoadedModules.Value.Select(module => NormalizeModuleName(module.name));

        ImpDropdown.Bind(
            "ModuleOverride",
            content,
            dropdownBinding,
            options,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Module Override",
                Description = "Makes the level to be generated from a single module.\nCaution! Can cause faulty levels.",
                Tooltip = tooltip
            },
            interactableInvert: true,
            interactableBindings: disabledBinding
        );
    }

    private void InitValuableSpawns(ImpBinaryBinding disabledBinding)
    {
        var options = Enum.GetValues(typeof(ValuableDirector.ValuableDebug))
            .Cast<ValuableDirector.ValuableDebug>()
            .OrderBy(value => value)
            .Select(type => type.ToString());

        ImpDropdown.Bind(
            "ValuableSpawns",
            content,
            Imperium.GameManager.CustomValuableSpawns,
            options,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Valuable Spawns",
                Description = "Customize how valuables will spawn in the level.",
                Tooltip = tooltip
            },
            interactableInvert: true,
            interactableBindings: disabledBinding
        );
    }

    private void InitModuleType(ImpBinaryBinding disabledBinding)
    {
        var options = Enum.GetValues(typeof(Module.Type))
            .Cast<Module.Type>()
            .OrderBy(value => value)
            .Select(type => type.ToString());

        ImpDropdown.Bind(
            "ModuleType",
            content,
            Imperium.GameManager.OverrideModuleType,
            options,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Title = "Module Type",
                Description = "The type of module to generate with the override module.",
                Tooltip = tooltip
            },
            interactableInvert: true,
            interactableBindings: disabledBinding
        );
    }

    private static string NormalizeModuleName(string moduleName) => moduleName.Replace("Module - ", "").Replace(" - ", " ");

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