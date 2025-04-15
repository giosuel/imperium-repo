#region

using System;
using System.Collections;
using Imperium.Core;
using Imperium.Interface.Common;
using Imperium.Types;
using Imperium.Util;
using Librarium.Binding;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

#endregion

namespace Imperium.Interface.ImperiumUI;

internal abstract class ImperiumWindow : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler,
    IPointerDownHandler
{
    protected ImpBinding<ImpTheme> theme;

    protected ImpTooltip tooltip;

    private CanvasGroup windowGroup;
    private Coroutine fadeAnimation;

    internal event Action onOpen;
    internal event Action onClose;
    internal event Action onFocus;

    protected Transform titleBox;
    protected RectTransform rect;

    private WindowDefinition windowDefinition;

    private ImperiumUI parent;

    internal InputAction openKeybind;

    private const float showAnimationDuration = 0.2f;
    private const float hideAnimationFadeDuration = 0.1f;
    private const float hideAnimationSnapDuration = 0.08f;

    public void InitWindow(
        ImpBinding<ImpTheme> themeBinding, WindowDefinition definition, ImpTooltip impTootip, ImperiumUI parentUI
    )
    {
        parent = parentUI;
        theme = themeBinding;
        windowDefinition = definition;
        tooltip = impTootip;

        titleBox = transform.Find("TitleBox");
        windowGroup = GetComponent<CanvasGroup>();
        rect = GetComponent<RectTransform>();
        if (titleBox) ImpButton.Bind("Close", titleBox, Close, theme: theme, isIconButton: true);

        onOpen += OnOpen;
        onOpen += SetWindowAnchors;

        onOpen += () =>
        {
            if (fadeAnimation != null) StopCoroutine(fadeAnimation);
            fadeAnimation = StartCoroutine(showAnimation());
        };

        onClose += () =>
        {
            // if (fadeAnimation != null) StopCoroutine(fadeAnimation);
            // fadeAnimation = StartCoroutine(animateOpacityTo(0.05f, 0, false));
            if (fadeAnimation != null) StopCoroutine(fadeAnimation);
            fadeAnimation = StartCoroutine(hideAnimation());
        };

        theme.onUpdate += OnThemePrimaryUpdate;
        theme.onUpdate += value =>
        {
            ImpThemeManager.Style(
                value,
                transform,
                // Window background color
                new StyleOverride("", Variant.BACKGROUND),
                // Titlebox border color
                new StyleOverride("TitleBox", Variant.DARKER),
                // Window border color
                new StyleOverride("Border", Variant.DARKER),
                new StyleOverride("Content", Variant.DARKER),
                new StyleOverride("Content/Border", Variant.DARKER)
            );

            if (titleBox)
            {
                // Window title
                ImpThemeManager.StyleText(
                    value,
                    transform,
                    new StyleOverride("TitleBox/Title", Variant.FOREGROUND)
                );
            }
        };

        InitWindow();

        // Style UI with the current theme
        OnThemePrimaryUpdate(theme.Value);

        windowGroup.alpha = 0;
        windowGroup.interactable = false;
        windowGroup.blocksRaycasts = false;
    }

    private IEnumerator hideAnimation()
    {
        var originalScale = rect.localScale;
        var elapsed = 0f;

        var overshootScaleX = originalScale.x * 1.2f;
        var squashScaleY = originalScale.y * 0.1f;

        while (elapsed < hideAnimationFadeDuration)
        {
            var t = elapsed / hideAnimationFadeDuration;

            windowGroup.alpha = Mathf.Lerp(1f, 0f, t);

            var scaleX = Mathf.Lerp(originalScale.x, overshootScaleX, t);
            var scaleY = Mathf.Lerp(originalScale.y, squashScaleY, t);
            rect.localScale = new Vector3(scaleX, scaleY, 1f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        var snapElapsed = 0f;
        var snapStart = rect.localScale;
        var snapEnd = new Vector3(0.1f, 0.1f, 1f);

        while (snapElapsed < hideAnimationSnapDuration)
        {
            var t = snapElapsed / hideAnimationSnapDuration;
            rect.localScale = Vector3.Lerp(snapStart, snapEnd, t);
            snapElapsed += Time.deltaTime;
            yield return null;
        }

        windowGroup.alpha = 0f;
        windowGroup.interactable = false;
        windowGroup.blocksRaycasts = false;
        rect.localScale = originalScale;
    }

    private IEnumerator showAnimation()
    {
        var originalScale = Vector3.one;

        var bounceScales = new[]
        {
            new(1.1f, 0.9f, 1f),
            new(0.95f, 1.05f, 1f),
            // new(1.05f, 0.95f, 1f),
            originalScale
        };

        var stepDuration = showAnimationDuration / (bounceScales.Length - 1);
        // var elapsedTotal = 0f;
        windowGroup.alpha = 1f;

        for (var i = 0; i < bounceScales.Length - 1; i++)
        {
            var start = bounceScales[i];
            var end = bounceScales[i + 1];
            var elapsedStep = 0f;

            while (elapsedStep < stepDuration)
            {
                var t = elapsedStep / stepDuration;
                rect.localScale = Vector3.LerpUnclamped(start, end, t);

                // windowGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTotal / showAnimationDuration);

                elapsedStep += Time.deltaTime;
                // elapsedTotal += Time.deltaTime;
                yield return null;
            }

            rect.localScale = end;
        }

        rect.localScale = originalScale;

        windowGroup.alpha = 1f;
        windowGroup.interactable = true;
        windowGroup.blocksRaycasts = true;
    }

    private IEnumerator animateOpacityTo(float duration, float targetAlpha, bool setInteractable = true)
    {
        var startAlpha = windowGroup.alpha;
        var elapsedTime = 0f;

        while (elapsedTime < duration)
        {
            windowGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsedTime / duration);

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        windowGroup.alpha = targetAlpha;
        windowGroup.interactable = setInteractable;
        windowGroup.blocksRaycasts = setInteractable;
    }

    private void OnDestroy()
    {
        if (openKeybind != null) openKeybind.performed -= OnKeybindOpen;
    }

    internal void OnKeybindOpen(InputAction.CallbackContext _)
    {
        if (!parent.IsOpen) parent.Open();

        if (windowDefinition.IsOpen)
        {
            FocusWindow();
            if (Imperium.Settings.Preferences.PlaySounds.Value) GameUtils.PlayClip(ImpAssets.OpenClick);
        }
        else
        {
            Open();
        }
    }

    internal void PlaceWindow(System.Numerics.Vector2 position, float scale, bool isOpen)
    {
        transform.position = new Vector2(position.X, position.Y);
        transform.localScale = new Vector3(scale * 1, scale * 1, 1);
        scaleFactor = scale;

        FocusWindow();

        // We don't want to call Open() here, as we don't want to call all the callbacka and play the noise
        if (isOpen)
        {
            onOpen?.Invoke();
            transform.gameObject.SetActive(true);
        }
    }

    protected void RegisterWidget<T>(Transform container, string path) where T : ImpWidget
    {
        container.Find(path).gameObject.AddComponent<T>().Init(theme, tooltip, ref onOpen, ref onClose, parent);
    }

    /// <summary>
    ///     Hides the window.
    /// </summary>
    public void Close()
    {
        // transform.gameObject.SetActive(false);
        // if (fadeAnimation != null) StopCoroutine(fadeAnimation);
        // fadeAnimation = StartCoroutine(animateOpacityTo(0.05f, 0, false));

        windowDefinition.IsOpen = false;

        onClose?.Invoke();
    }

    /// <summary>
    ///     Show the window.
    /// </summary>
    public void Open()
    {
        FocusWindow();

        // Set position if is opened the first time
        windowDefinition.Position = new System.Numerics.Vector2(transform.position.x, transform.position.y);
        windowDefinition.IsOpen = true;

        onOpen?.Invoke();
        if (Imperium.Settings.Preferences.PlaySounds.Value) GameUtils.PlayClip(ImpAssets.OpenClick);
    }

    /// <summary>
    ///     Called by <see cref="ImperiumUI" /> whever it opens.
    /// </summary>
    public void InvokeOnOpen() => onOpen?.Invoke();

    private void FocusWindow()
    {
        transform.SetAsLastSibling();
        onFocus?.Invoke();
    }

    private float scaleFactor = 1f;

    /// <summary>
    /// Sets the window's anchors based on the screen side the window is currently placed without moving the window.
    /// </summary>
    private void SetWindowAnchors()
    {
        var windowWorldPosition = transform.position;
        var posX = transform.position.x;

        if (posX < Screen.width / 2f)
        {
            rect.anchorMin = new Vector2(0, 0.5f);
            rect.anchorMax = new Vector2(0, 0.5f);
        }
        else
        {
            rect.anchorMin = new Vector2(1, 0.5f);
            rect.anchorMax = new Vector2(1, 0.5f);
        }

        // Restore original window position
        transform.position = windowWorldPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Imperium.InputBindings.StaticMap["Alt"].IsPressed())
        {
            var delta = eventData.delta.magnitude * 0.002f;
            var windowOrigin = new Vector2(transform.position.x, transform.position.y);
            if ((windowOrigin - eventData.position).magnitude <
                (windowOrigin - eventData.position + eventData.delta).magnitude) delta *= -1;
            scaleFactor = Math.Clamp(scaleFactor + delta, 0.5f, 1f);

            transform.localScale = Vector3.one * scaleFactor + new Vector3(0, 0, 1);

            windowDefinition.ScaleFactor = scaleFactor;
        }
        else
        {
            transform.position = (Vector2)transform.position + eventData.delta;
            windowDefinition.Position = new System.Numerics.Vector2(transform.position.x, transform.position.y);
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        FocusWindow();
        Cursor.lockState = CursorLockMode.Confined;

        if (windowGroup)
        {
            if (fadeAnimation != null) StopCoroutine(fadeAnimation);
            fadeAnimation = StartCoroutine(animateOpacityTo(0.15f, 0.9f));
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Cursor.lockState = CursorLockMode.None;

        if (windowGroup)
        {
            if (fadeAnimation != null) StopCoroutine(fadeAnimation);
            fadeAnimation = StartCoroutine(animateOpacityTo(0.15f, 1f));
        }

        SetWindowAnchors();
    }

    public void OnPointerDown(PointerEventData eventData) => FocusWindow();

    protected abstract void InitWindow();

    protected void CloseParent() => parent.Close();

    protected virtual void OnThemePrimaryUpdate(ImpTheme themeUpdated)
    {
    }

    protected virtual void OnClose()
    {
    }

    protected virtual void OnOpen()
    {
    }

    public virtual bool CanOpen()
    {
        return true;
    }
}