#region

using System;
using System.Collections.Generic;
using System.Linq;
using Imperium.Core.Lifecycle;
using Imperium.Interface.Common;
using Imperium.Types;
using Librarium.Binding;
using TMPro;
using UnityEngine;

#endregion

namespace Imperium.Interface.SpawningUI;

public class SpawningObjectEntry : MonoBehaviour
{
    private GameObject selectedCover;

    private SpawnObjectType spawnType { get; set; }

    private string objectName;
    private string objectNameNormalized;

    internal void Init(
        SpawnObjectType type,
        string spawnObjectName,
        Action onClick,
        Action<Vector2> onHover,
        Dictionary<SpawnObjectType, string> typeDisplayNameMap,
        ImpBinding<ImpTheme> themeBinding
    )
    {
        spawnType = type;
        objectName = spawnObjectName;

        objectNameNormalized = NormalizeName(spawnObjectName);

        ImpButton.Bind("", transform, () => onClick?.Invoke(), theme: themeBinding);

        selectedCover = transform.Find("Selected").gameObject;
        selectedCover.SetActive(false);

        transform.Find("Name").GetComponent<TMP_Text>().text = spawnObjectName;
        transform.Find("Type").GetComponent<TMP_Text>().text = typeDisplayNameMap.GetValueOrDefault(type, "");

        gameObject.AddComponent<ImpInteractable>().onOver += onHover;
        themeBinding.OnUpdate += OnThemePrimaryUpdate;
    }

    private void OnThemePrimaryUpdate(ImpTheme themeUpdate)
    {
        ImpThemeManager.Style(
            themeUpdate,
            transform,
            new StyleOverride("", Variant.FOREGROUND),
            new StyleOverride("Selected", Variant.FADED)
        );
    }

    internal void SetShown(bool isShown)
    {
        gameObject.SetActive(isShown);
        SetSelected(false);
    }

    internal void SetSelected(bool isSelected)
    {
        selectedCover.SetActive(isSelected);
    }

    internal bool OnInput(string inputText)
    {
        inputText = NormalizeName(inputText);

        var isShown = !string.IsNullOrEmpty(inputText) && objectNameNormalized.Contains(inputText);

        gameObject.SetActive(isShown);
        if (!isShown) SetSelected(false);

        return isShown;
    }

    private static string NormalizeName(string input)
    {
        return new string(input.Trim().ToLower().Where(char.IsLetterOrDigit).ToArray());
    }
}