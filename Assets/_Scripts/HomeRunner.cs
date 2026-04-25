using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HomeRunner : MonoBehaviour
{

    public Image FadeInCover;
    public float fadeInDuration;
    public float fadeInDelay;

    private void Start()
    {
        // Optional: auto-start fade on play
        StartFadeOut();
    }

    public void StartFadeOut()
    {
        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        if (FadeInCover == null)
            yield break;

        yield return new WaitForSeconds(fadeInDelay);

        float startAlpha = FadeInCover.color.a;
        float time = 0f;

        while (time < fadeInDuration)
        {
            time += Time.deltaTime;
            float t = time / fadeInDuration;

            Color newColor = FadeInCover.color;
            newColor.a = Mathf.Lerp(startAlpha, 0f, t);
            FadeInCover.color = newColor;

            yield return null;
        }

        // ensure fully transparent at end
        Color finalColor = FadeInCover.color;
        finalColor.a = 0f;
        FadeInCover.color = finalColor;
        FadeInCover.gameObject.SetActive(false);
    }
}
