#region

using System;
using Imperium.Types;
using Librarium.Binding;
using UnityEngine;

#endregion

namespace Imperium.Interface;

public abstract class ImpWidget : MonoBehaviour
{
    protected IBinding<ImpTheme> theme;
    protected event Action onOpen;
    protected event Action onClose;
    protected ImpTooltip tooltip { get; private set; }

    private ImperiumUI.ImperiumUI parent;

    internal void Init(IBinding<ImpTheme> themeBinding, ImpTooltip tooltipReference)
    {
        theme = themeBinding;
        tooltip = tooltipReference;
        theme.onPrimaryUpdate += OnThemeUpdate;

        InitWidget();
    }

    internal void Init(
        IBinding<ImpTheme> themeBinding,
        ImpTooltip tooltipReference,
        ref Action onOpenAction,
        ref Action onCloseAction,
        ImperiumUI.ImperiumUI widgetParent
    )
    {
        parent = widgetParent;

        onOpenAction += () => onOpen?.Invoke();
        onCloseAction += () => onClose?.Invoke();

        onOpenAction += OnOpen;
        onCloseAction += OnClose;

        Init(themeBinding, tooltipReference);
    }

    protected void CloseParent() => parent?.Close();

    protected virtual void OnOpen()
    {
    }

    protected virtual void OnClose()
    {
    }

    protected virtual void InitWidget()
    {
    }

    protected virtual void OnThemeUpdate(ImpTheme themeUpdate)
    {
    }
}