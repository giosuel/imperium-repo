#region

using System;
using Imperium.Util;
using TMPro;

#endregion

namespace Imperium.Interface.Common;

/// <summary>
///     Clickable TMP text component that also supports has a custom hover effect
///     Note: Default hover effect is underlined text.
/// </summary>
internal class ImpClickableText : ImpInteractable
{
    private Action callback;
    private TMP_Text textComponent;
    private string text;

    private Func<string, string> onHoverEffect;

    public void Init(string value, Action action, Func<string, string> hoverEffect = null)
    {
        textComponent = GetComponent<TMP_Text>();
        callback = action;
        text = value;
        onHoverEffect = hoverEffect ?? RichText.Underlined;
        textComponent.text = text;

        onEnter += _ => textComponent.text = onHoverEffect(text);
        onExit += () => textComponent.text = text;
        onClick += () =>
        {
            callback.Invoke();
            textComponent.text = text;
        };
    }
}