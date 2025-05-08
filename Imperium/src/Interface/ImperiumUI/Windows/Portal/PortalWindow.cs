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

    private GameObject modTitleTemplate;
    private GameObject sectionTitleTemplate;
    private GameObject toggleContainerTemplate;
    private GameObject buttonTemplate;
    private GameObject toggleTemplate;
    private GameObject sliderTemplate;
    private GameObject inputTemplate;
    private GameObject dropdownTemplate;

    private readonly List<GameObject> interfaceElements = [];

    protected override void InitWindow()
    {
        content = transform.Find("Content/Viewport/Content");

        modTitleTemplate = content.Find("ModTitle").gameObject;
        modTitleTemplate.SetActive(false);

        sectionTitleTemplate = content.Find("SectionTitle").gameObject;
        sectionTitleTemplate.SetActive(false);

        toggleContainerTemplate = content.Find("ToggleContainer").gameObject;
        toggleContainerTemplate.SetActive(false);

        buttonTemplate = content.Find("ButtonTemplate").gameObject;
        buttonTemplate.SetActive(false);

        toggleTemplate = toggleContainerTemplate.transform.Find("Template").gameObject;
        toggleTemplate.SetActive(false);

        sliderTemplate = content.Find("SliderTemplate").gameObject;
        sliderTemplate.SetActive(false);

        inputTemplate = content.Find("InputTemplate").gameObject;
        inputTemplate.SetActive(false);

        dropdownTemplate = content.Find("DropdownTemplate").gameObject;
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

        BuildPortal(Imperium.PortalManager.RuntimePortal, true);
        Imperium.PortalManager.RegisteredPortals.Values.Do(portal => BuildPortal(portal));
    }

    private void BuildPortal(ImpPortal portal, bool isRuntimePortal = false)
    {
        var portalTitle = Instantiate(modTitleTemplate, content);
        portalTitle.SetActive(true);
        interfaceElements.Add(portalTitle);
        portalTitle.GetComponent<TMP_Text>().text = portal.Name;

        portal.Sections.Values.Do(BuildSection);
    }

    private void BuildSection(ImpPortalSection section)
    {
        var sectionTitle = Instantiate(sectionTitleTemplate, content);
        sectionTitle.SetActive(true);
        interfaceElements.Add(sectionTitle);
        sectionTitle.GetComponent<TMP_Text>().text = section.Name;

        var toggleContainer = Instantiate(toggleContainerTemplate, content);
        interfaceElements.Add(toggleContainer);
        toggleContainer.SetActive(true);

        section.Elements.Do(element => interfaceElements.Add(BuildElement(element, toggleContainer.transform)));
    }

    private GameObject BuildElement(ImpPortalElement portalElement, Transform toggleContainer)
    {
        return portalElement switch
        {
            ImpPortalButton button => BuildButton(button),
            ImpPortalDropdown dropdown => BuildDropdown(dropdown),
            ImpPortalToggle toggle => BuildToggle(toggle, toggleContainer),
            ImpPortalTextField or ImpPortalNumberField or ImpPortalDecimalField => BuildInputField(portalElement),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    private GameObject BuildButton(ImpPortalButton button)
    {
        var buttonObj = Instantiate(buttonTemplate, content);
        buttonObj.SetActive(false);

        var tooltipDefinition = GetTooltipDefinition(button);

        ImpButton.Bind(
            "",
            buttonObj.transform,
            onClick: () => button.onClick(),
            theme: theme,
            tooltipDefinition: tooltipDefinition,
            interactableBindings: button.interactableBinding
        );

        return buttonObj;
    }

    private GameObject BuildDropdown(ImpPortalDropdown dropdown)
    {
        var dropdownObj = Instantiate(dropdownTemplate, content);
        dropdownObj.SetActive(false);

        var tooltipDefinition = GetTooltipDefinition(dropdown);

        ImpDropdown.Bind(
            "",
            dropdownObj.transform,
            valueBinding: dropdown.valueBinding,
            options: dropdown.options,
            placeholder: dropdown.placeholder,
            allowReset: dropdown.allowReset,
            theme: theme,
            tooltipDefinition: tooltipDefinition,
            interactableBindings: dropdown.interactableBinding
        );

        return dropdownObj;
    }

    private GameObject BuildToggle(ImpPortalToggle toggle, Transform container)
    {
        var toggleObj = Instantiate(toggleTemplate, container);
        toggleObj.SetActive(false);

        var tooltipDefinition = GetTooltipDefinition(toggle);

        ImpToggle.Bind(
            "",
            toggleObj.transform,
            valueBinding: toggle.valueBinding,
            theme: theme,
            tooltipDefinition: tooltipDefinition,
            interactableBindings: toggle.interactableBinding
        );

        return toggleObj;
    }

    private GameObject BuildInputField(ImpPortalElement inputField)
    {
        var inputFieldObj = Instantiate(inputTemplate, content);
        inputFieldObj.SetActive(true);

        var tooltipDefinition = GetTooltipDefinition(inputField);

        switch (inputField)
        {
            case ImpPortalTextField textField:
                ImpInput.Bind(
                    "",
                    inputFieldObj.transform,
                    valueBinding: textField.valueBinding,
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

        return inputFieldObj;
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
}