#region

using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Bootstrap;
using HarmonyLib;
using Imperium.API.Types.Portals;
using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Librarium.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.ImperiumUI.Windows.Portal;

internal class PortalWindow : ImperiumWindow
{
    private Transform content;

    private GameObject portalTitleTemplate;
    private GameObject portalContainerTemplate;
    private GameObject sectionTitleTemplate;
    private GameObject sectionContainerTemplate;
    private GameObject toggleContainerTemplate;
    private GameObject buttonContainerTemplate;
    private GameObject buttonTemplate;
    private GameObject toggleTemplate;
    private GameObject sliderTemplate;
    private GameObject inputTemplate;
    private GameObject dropdownTemplate;

    private readonly List<GameObject> interfaceElements = [];

    // Cached titles for theme updates
    private readonly List<GameObject> titleObjects = [];

    protected override void InitWindow()
    {
        content = transform.Find("Content/Viewport/Content");

        portalTitleTemplate = content.Find("PortalTitle").gameObject;
        portalTitleTemplate.SetActive(false);

        portalContainerTemplate = content.Find("PortalContainer").gameObject;
        portalContainerTemplate.SetActive(false);

        sectionTitleTemplate = portalContainerTemplate.transform.Find("SectionTitle").gameObject;
        sectionTitleTemplate.SetActive(false);

        sectionContainerTemplate = portalContainerTemplate.transform.Find("SectionContainer").gameObject;
        sectionContainerTemplate.SetActive(false);

        toggleContainerTemplate = sectionContainerTemplate.transform.Find("ToggleContainer").gameObject;
        toggleContainerTemplate.SetActive(false);

        buttonContainerTemplate = sectionContainerTemplate.transform.Find("ButtonContainer").gameObject;
        buttonContainerTemplate.SetActive(false);

        buttonTemplate = buttonContainerTemplate.transform.Find("Template").gameObject;
        buttonTemplate.SetActive(false);

        toggleTemplate = toggleContainerTemplate.transform.Find("Template").gameObject;
        toggleTemplate.SetActive(false);

        sliderTemplate = sectionContainerTemplate.transform.Find("SliderTemplate").gameObject;
        sliderTemplate.SetActive(false);

        inputTemplate = sectionContainerTemplate.transform.Find("InputTemplate").gameObject;
        inputTemplate.SetActive(false);

        dropdownTemplate = sectionContainerTemplate.transform.Find("DropdownTemplate").gameObject;
        dropdownTemplate.SetActive(false);

        BuildPortal();
    }

    private void ClearPortals()
    {
        interfaceElements.Do(Destroy);
        interfaceElements.Clear();
    }

    private void BuildPortal()
    {
        ClearPortals();

        if (Imperium.PortalManager.RuntimePortal.Elements > 0) BuildPortal(Imperium.PortalManager.RuntimePortal, true);

        Imperium.PortalManager.RegisteredPortals.Values
            .Where(portal => portal.Elements > 0)
            .Do(portal => BuildPortal(portal));
    }

    private void BuildPortal(ImpPortal portal, bool isRuntimePortal = false)
    {
        var portalTitle = Instantiate(portalTitleTemplate, content);
        portalTitle.SetActive(true);
        portalTitle.transform.Find("Text").GetComponent<TMP_Text>().text = portal.Name;
        titleObjects.Add(portalTitle);
        interfaceElements.Add(portalTitle);

        var portalContainer = Instantiate(portalContainerTemplate, content);
        portalContainer.SetActive(true);
        interfaceElements.Add(portalContainer);

        ImpButton.CreateCollapse("Arrow", portalTitle.transform, portalContainer.transform);

        portal.Sections.Values.Do(section => BuildSection(section, portalContainer.transform));
    }

    private void BuildSection(ImpPortalSection section, Transform portalContainer)
    {
        var sectionTitle = Instantiate(sectionTitleTemplate, portalContainer);
        sectionTitle.SetActive(true);
        sectionTitle.transform.Find("Text").GetComponent<TMP_Text>().text = section.Name;
        titleObjects.Add(sectionTitle);

        var sectionContainer = Instantiate(sectionContainerTemplate, portalContainer);
        sectionContainer.SetActive(true);

        var toggleContainer = Instantiate(toggleContainerTemplate, sectionContainer.transform);
        toggleContainer.SetActive(true);

        var buttonContainer = Instantiate(buttonContainerTemplate, sectionContainer.transform);
        buttonContainer.SetActive(true);

        ImpButton.CreateCollapse("Arrow", sectionTitle.transform, sectionContainer.transform);

        section.Elements.Do(element =>
        {
            BuildElement(element, sectionContainer.transform, toggleContainer.transform, buttonContainer.transform);
        });

        toggleContainer.transform.SetAsFirstSibling();
        buttonContainer.transform.SetAsLastSibling();
    }

    private void BuildElement(
        ImpPortalElement portalElement,
        Transform sectionContainer,
        Transform toggleContainer,
        Transform buttonContainer
    )
    {
        switch (portalElement)
        {
            case ImpPortalButton button:
                BuildButton(button, buttonContainer);
                break;
            case ImpPortalDropdown dropdown:
                BuildDropdown(dropdown, sectionContainer);
                break;
            case ImpPortalSlider slider:
                BuildSlider(slider, sectionContainer);
                break;
            case ImpPortalToggle toggle:
                BuildToggle(toggle, toggleContainer);
                break;
            case ImpPortalTextField or ImpPortalNumberField or ImpPortalDecimalField:
                BuildInputField(portalElement, sectionContainer);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void BuildButton(ImpPortalButton button, Transform container)
    {
        var buttonObj = Instantiate(buttonTemplate, container);
        buttonObj.SetActive(true);

        var tooltipDefinition = GetTooltipDefinition(button);

        ImpButton.Bind(
            "",
            buttonObj.transform,
            label: button.label,
            onClick: () => button.onClick(),
            theme: theme,
            tooltipDefinition: tooltipDefinition,
            interactableBindings: button.interactableBinding
        );
    }

    private void BuildDropdown(ImpPortalDropdown dropdown, Transform container)
    {
        var dropdownObj = Instantiate(dropdownTemplate, container);
        dropdownObj.SetActive(true);

        var tooltipDefinition = GetTooltipDefinition(dropdown);

        ImpDropdown.Bind(
            "",
            dropdownObj.transform,
            valueBinding: dropdown.valueBinding,
            options: dropdown.options,
            label: dropdown.label,
            placeholder: dropdown.placeholder,
            allowReset: dropdown.allowReset,
            theme: theme,
            tooltipDefinition: tooltipDefinition,
            interactableBindings: dropdown.interactableBinding
        );
    }

    private void BuildToggle(ImpPortalToggle toggle, Transform container)
    {
        var toggleObj = Instantiate(toggleTemplate, container);
        toggleObj.SetActive(true);

        var tooltipDefinition = GetTooltipDefinition(toggle);

        ImpToggle.Bind(
            "",
            toggleObj.transform,
            valueBinding: toggle.valueBinding,
            label: toggle.label,
            theme: theme,
            tooltipDefinition: tooltipDefinition,
            interactableBindings: toggle.interactableBinding
        );
    }

    private void BuildSlider(ImpPortalSlider slider, Transform container)
    {
        var sliderObj = Instantiate(sliderTemplate, container);
        sliderObj.SetActive(true);

        var tooltipDefinition = GetTooltipDefinition(slider);

        ImpSlider.Bind(
            "",
            sliderObj.transform,
            valueBinding: slider.valueBinding,
            minValue: slider.minValue,
            maxValue: slider.maxValue,
            label: slider.label,
            debounceTime: slider.debounceTime,
            valueUnit: slider.valueUnit,
            handleFormatter: slider.handleFormatter,
            useWholeNumbers: slider.useWholeNumbers,
            negativeIsDefault: slider.negativeIsDefault,
            allowReset: slider.allowReset,
            theme: theme,
            tooltipDefinition: tooltipDefinition,
            interactableBindings: slider.interactableBinding
        );
    }

    private void BuildInputField(ImpPortalElement inputField, Transform container)
    {
        var inputFieldObj = Instantiate(inputTemplate, container);
        inputFieldObj.SetActive(true);

        var tooltipDefinition = GetTooltipDefinition(inputField);

        switch (inputField)
        {
            case ImpPortalTextField textField:
                ImpInput.Bind(
                    "",
                    inputFieldObj.transform,
                    valueBinding: textField.valueBinding,
                    label: textField.label,
                    placeholder: textField.placeholder,
                    updateOnSubmit: textField.updateOnSubmit,
                    allowReset: textField.allowReset,
                    theme: theme,
                    tooltipDefinition: tooltipDefinition,
                    interactableBindings: inputField.interactableBinding
                );
                break;
            case ImpPortalNumberField numberField:
                ImpInput.Bind(
                    "",
                    inputFieldObj.transform,
                    valueBinding: numberField.valueBinding,
                    label: numberField.label,
                    placeholder: numberField.placeholder,
                    updateOnSubmit: numberField.updateOnSubmit,
                    allowReset: numberField.allowReset,
                    min: numberField.minValue,
                    max: numberField.maxValue,
                    theme: theme,
                    tooltipDefinition: tooltipDefinition,
                    interactableBindings: inputField.interactableBinding
                );
                break;
            case ImpPortalDecimalField decimalField:
                ImpInput.Bind(
                    "",
                    inputFieldObj.transform,
                    valueBinding: decimalField.valueBinding,
                    label: decimalField.label,
                    placeholder: decimalField.placeholder,
                    updateOnSubmit: decimalField.updateOnSubmit,
                    allowReset: decimalField.allowReset,
                    min: decimalField.minValue,
                    max: decimalField.maxValue,
                    theme: theme,
                    tooltipDefinition: tooltipDefinition,
                    interactableBindings: inputField.interactableBinding
                );
                break;
        }
    }

    private TooltipDefinition GetTooltipDefinition(ImpPortalElement element)
    {
        if (element.elementTooltip == null) return null;

        return new TooltipDefinition
        {
            Title = element.elementTooltip.title,
            Description = element.elementTooltip.description,
            Tooltip = tooltip
        };
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        foreach (var titleObj in titleObjects)
        {
            ImpThemeManager.Style(
                themeUpdate,
                titleObj.transform,
                new StyleOverride("", Variant.DARKER)
            );
        }
    }
}