using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class HomeUIManager : MonoBehaviour
{

    public Image FadeInCover;
    public float fadeInDuration;
    public float fadeInDelay;

    public void FadeInHomescreen()
    {
        StartCoroutine(FadeInHome());
    }

    private IEnumerator FadeInHome()
    {
        if (FadeInCover == null)
            yield break;

        FadeInCover.gameObject.SetActive(true);

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

        Color finalColor = FadeInCover.color;
        finalColor.a = 0f;
        FadeInCover.color = finalColor;
        FadeInCover.gameObject.SetActive(false);
    }
}
