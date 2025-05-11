#region

using System.Linq;
using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Librarium.Binding;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface;

public class ImperiumDock : BaseUI
{
    internal void RegisterDockButton<T>(
        string buttonPath,
        ImpInterfaceManager dockInterfaceManager,
        string interfaceName,
        string interfaceDescription,
        params IBinding<bool>[] canOpenBindings
    ) where T : BaseUI
    {
        var buttonObj = container.Find(buttonPath);
        var buttonImage = buttonObj.GetComponent<Image>();
        buttonImage.enabled = dockInterfaceManager.IsOpen<T>() && canOpenBindings.All(binding => binding.Value);

        ImpButton.Bind(
            "",
            buttonObj,
            () =>
            {
                if (canOpenBindings.All(binding => binding.Value))
                {
                    dockInterfaceManager.Open<T>();
                }
            },
            isIconButton: true,
            playClickSound: false,
            theme: theme,
            tooltipDefinition: new TooltipDefinition
            {
                Tooltip = tooltip,
                Title = interfaceName,
                Description = interfaceDescription,
                HasAccess = true
            },
            interactableBindings: canOpenBindings
        );

        dockInterfaceManager.OpenInterface.OnUpdate += selectedInterface =>
        {
            buttonImage.enabled = selectedInterface && selectedInterface.GetType() == typeof(T);
        };
    }

    protected override void OnThemePrimaryUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            container,
            new StyleOverride("", Variant.BACKGROUND),
            new StyleOverride("Border", Variant.DARKER)
        );
    }

    protected override void OnOpen(bool wasOpen)
    {
        if (rect && !wasOpen) StartCoroutine(ImpUtils.Interface.SlideInAnimation(rect, Vector2.right));
    }
}