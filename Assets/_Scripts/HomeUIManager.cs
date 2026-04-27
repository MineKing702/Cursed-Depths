using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CursedDepths.Core.Events;
using CursedDepths.Core.Settings;

public class HomeUIManager : MonoBehaviour
{
    public Image FadeInCover;
    public float fadeInDuration = 1f;
    public float fadeInDelay = 0.25f;

    public Animator HomeUIParentAnim;
    public Animator SettingsUIParentAnim;

    public Slider MasterVol;
    public Slider MusicVol;
    public Slider EffectsVol;

    private void OnEnable()
    {
        GameEvents.GameStartupRequested += FadeInHomescreen;
        GameEvents.PlayerSettingsLoaded += UpdateVolumeSliders;
    }

    private void OnDisable()
    {
        GameEvents.GameStartupRequested -= FadeInHomescreen;
    }

    private void FadeInHomescreen()
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
            float t = Mathf.SmoothStep(0f, 1f, time / fadeInDuration);

            Color color = FadeInCover.color;
            color.a = Mathf.Lerp(startAlpha, 0f, t);
            FadeInCover.color = color;

            yield return null;
        }

        Color finalColor = FadeInCover.color;
        finalColor.a = 0f;
        FadeInCover.color = finalColor;
        FadeInCover.gameObject.SetActive(false);

        GameEvents.FinishGameStartup();
    }

    public void OpenSettings()
    {
        HomeUIParentAnim.Play("UiSlideFadeOut");

        SettingsUIParentAnim.Play("UiSlideFadeIn");
    }

    public void UpdateVolumeSliders(PlayerSettings settings)
    {
        MasterVol.value = settings.MasterVolume / 100;
        MusicVol.value = settings.MusicVolume / 100;
        EffectsVol.value = settings.SoundEffects / 100;
    }
}