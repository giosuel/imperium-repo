#region

using System.Collections;
using Imperium.Types;
using UnityEngine;

#endregion

namespace Imperium.Interface;

internal class ImpHighlighter : ImpWidget
{
    private RectTransform panelRect;
    private CanvasGroup panelGroup;

    private Coroutine showAnimationCoroutine;

    private bool isShown;

    private Transform realParent;
    private Transform fakeParent;

    private const float highlightFadeInTime = 0.2f;
    private const float highlightScaleTime = 0.2f;
    private const float highlightPauseTime = 1f;
    private const float highlightFadeOutTime = 0.4f;

    private void Awake()
    {
        realParent = transform.parent;

        panelRect = transform.GetComponent<RectTransform>();
        panelGroup = panelRect.GetComponent<CanvasGroup>();

        panelGroup.alpha = 0;
    }

    internal void Highlight(RectTransform elementTransform)
    {
        PlaceHighlighter(elementTransform);

        if (showAnimationCoroutine != null) StopCoroutine(showAnimationCoroutine);
        showAnimationCoroutine = StartCoroutine(highlightAnimation());
    }

    private IEnumerator highlightAnimation()
    {
        isShown = true;

        panelGroup.alpha = 0;
        panelRect.localScale = Vector3.one;

        for (float t = 0; t < highlightFadeInTime; t += Time.deltaTime)
        {
            panelGroup.alpha = Mathf.Lerp(0, 1, t / highlightFadeInTime);
            yield return null;
        }
        panelGroup.alpha = 1;

        var originalSize = panelRect.sizeDelta;
        var paddedSize = originalSize + new Vector2(10, 10) * 2;
        for (float t = 0; t < highlightScaleTime; t += Time.deltaTime)
        {
            panelRect.sizeDelta = Vector2.Lerp(originalSize, paddedSize, t / highlightScaleTime);
            yield return null;
        }

        for (float t = 0; t < highlightScaleTime; t += Time.deltaTime)
        {
            panelRect.sizeDelta = Vector2.Lerp(paddedSize, originalSize, t / highlightScaleTime);
            yield return null;
        }

        panelRect.sizeDelta = originalSize;

        yield return new WaitForSeconds(highlightPauseTime);

        for (float t = 0; t < highlightFadeOutTime; t += Time.deltaTime)
        {
            panelGroup.alpha = Mathf.Lerp(1, 0, t / highlightFadeOutTime);
            yield return null;
        }
        panelGroup.alpha = 0;
        isShown = false;
    }

    private void PlaceHighlighter(RectTransform elementRect)
    {
        fakeParent = elementRect;
        panelRect.SetParent(fakeParent, worldPositionStays: false);

        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;

        panelRect.offsetMin = Vector2.one * -3;
        panelRect.offsetMax = Vector2.one * 3;

        panelRect.SetParent(realParent, worldPositionStays: true);
    }

    private void Update()
    {
        if (isShown && fakeParent) transform.position = fakeParent.position;
    }

    protected override void OnThemeUpdate(ImpTheme themeUpdate)
    {
        // ImpThemeManager.Style(
        //     themeUpdate,
        //     panel,
        //     new StyleOverride("", Variant.BACKGROUND),
        //     new StyleOverride("Border", Variant.DARKER)
        // );
    }
}