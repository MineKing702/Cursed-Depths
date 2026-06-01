using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public sealed class ScreenFader : MonoBehaviour
{
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float defaultDuration = 0.75f;

    private void Awake()
    {
        EnsureCanvasGroup();
        SetAlpha(0f);
        SetBlocking(false);
    }

    public IEnumerator FadeOut(float? durationOverride = null)
    {
        yield return FadeTo(1f, durationOverride);
    }

    public IEnumerator FadeIn(float? durationOverride = null)
    {
        yield return FadeTo(0f, durationOverride);
    }

    private IEnumerator FadeTo(float targetAlpha, float? durationOverride)
    {
        EnsureCanvasGroup();
        SetBlocking(true);

        float duration = Mathf.Max(0f, durationOverride ?? defaultDuration);
        float startAlpha = canvasGroup.alpha;

        if (duration <= Mathf.Epsilon)
        {
            SetAlpha(targetAlpha);
            SetBlocking(targetAlpha > 0f);
            yield break;
        }

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            SetAlpha(Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration));
            yield return null;
        }

        SetAlpha(targetAlpha);
        SetBlocking(targetAlpha > 0f);
    }

    private void EnsureCanvasGroup()
    {
        if (canvasGroup != null)
        {
            return;
        }

        CanvasGroup existingCanvasGroup = GetComponentInChildren<CanvasGroup>(true);
        if (existingCanvasGroup != null)
        {
            canvasGroup = existingCanvasGroup;
            return;
        }

        GameObject canvasObject = new GameObject("Screen Fade Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = short.MaxValue;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);

        canvasGroup = canvasObject.GetComponent<CanvasGroup>();

        GameObject imageObject = new GameObject("Black Fade Image", typeof(RectTransform), typeof(Image));
        imageObject.transform.SetParent(canvasObject.transform, false);

        RectTransform rectTransform = imageObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;

        Image image = imageObject.GetComponent<Image>();
        image.color = Color.black;
    }

    private void SetAlpha(float alpha)
    {
        canvasGroup.alpha = Mathf.Clamp01(alpha);
    }

    private void SetBlocking(bool isBlocking)
    {
        canvasGroup.blocksRaycasts = isBlocking;
        canvasGroup.interactable = isBlocking;
    }
}
