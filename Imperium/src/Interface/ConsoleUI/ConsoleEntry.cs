#region

using System;
using Imperium.Console;
using Imperium.Console.Commands;
using Imperium.Interface.Common;
using Imperium.Types;
using JetBrains.Annotations;
using Librarium.Binding;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

#endregion

namespace Imperium.Interface.ConsoleUI;

public class ConsoleEntry : MonoBehaviour
{
    internal ImpCommand Command { get; private set; }

    private GameObject selectedCover;
    private GameObject disabledCover;
    private TMP_Text displayName;
    private TMP_Text displayType;
    private Image iconImage;
    private Button iconButton;

    private Action onEntryClick;

    internal void Init(IBinding<ImpTheme> themeBinding, Action<Vector2> onHover)
    {
        ImpButton.Bind("", transform, OnButtonClick, theme: themeBinding);

        selectedCover = transform.Find("Selected").gameObject;
        selectedCover.SetActive(false);

        disabledCover = transform.Find("Disabled").gameObject;
        disabledCover.SetActive(false);

        displayName = transform.Find("Name").GetComponent<TMP_Text>();
        displayType = transform.Find("Type").GetComponent<TMP_Text>();
        iconImage = transform.Find("Tab/Icon").GetComponent<Image>();

        iconButton = transform.Find("Tab").GetComponent<Button>();
        iconButton.onClick.AddListener(OnIconClick);

        gameObject.AddComponent<ImpInteractable>().onOver += onHover;

        OnThemeUpdate(themeBinding.Value);
        themeBinding.OnUpdate += OnThemeUpdate;
    }

    internal void SetEntry([CanBeNull] ImpCommand command, ConsoleQuery query, Action onClick)
    {
        if (command == null)
        {
            gameObject.SetActive(false);
            return;
        }

        Command = command;
        onEntryClick = onClick;

        selectedCover.SetActive(false);
        disabledCover.SetActive(!command.IsEnabled());

        displayName.text = Command.GetDisplayName(query);
        displayType.text = Command.DisplayType;
        iconImage.sprite = Command.Icon;

        gameObject.SetActive(true);
    }

    internal void HideEntry() => gameObject.SetActive(false);

    internal void SetSelected(bool isSelected)
    {
        selectedCover.SetActive(isSelected);
    }

    private void OnButtonClick() => onEntryClick?.Invoke();
    private void OnIconClick() => Command?.OnIconClick();

    private void OnThemeUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("", Variant.BACKGROUND),
            new StyleOverride("Selected", Variant.DARKEST),
            new StyleOverride("Tab", Variant.DARKEST)
        );
    }
}